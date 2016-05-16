using System;
using System.Threading.Tasks;
using System.Data.Entity.Migrations;
using AFT.RegoV2.Core.Common.Attributes;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Games;
using AFT.RegoV2.Core.Common.Exceptions;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public sealed class GameCommands : MarshalByRefObject, IGameCommands
    {
        private readonly IEventBus _eventBus;
        private readonly IGameRepository _repository;
        private readonly IGameWalletOperations _walletOperations;


        public GameCommands(
            IGameRepository repository,
            IGameWalletOperations walletCommands,
            IEventBus eventBus)
        {
            _eventBus = eventBus;
            _walletOperations = walletCommands;
            _repository = repository;
        }
        
        /// <summary>
        /// Places a bet
        /// </summary>
        /// <param name="actionData">Information about the game action</param>
        /// <param name="context"></param>
        /// <param name="token">Security token</param>
        /// <returns>Game action ID</returns>
        Guid IGameCommands.PlaceBet([NotNull] GameActionData actionData, [NotNull] GameActionContext context, [NotNull] TokenData token)
        {
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = _repository.GetOrCreateRound(actionData.RoundId, token);

                var walletTransactionId = _walletOperations.PlaceBet(token.PlayerId, token.GameId, round.Data.Id, actionData.Amount);

                var placeBetGameActionId = round.Place(actionData.Amount, actionData.Description, walletTransactionId, actionData.TokenId, actionData.ExternalTransactionId);

                _repository.Rounds.AddOrUpdate(x => x.ExternalRoundId, round.Data);
                _repository.SaveChanges();

                _eventBus.Publish(new BetPlaced
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = placeBetGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });
                
                scope.Complete();

                return placeBetGameActionId;
            }

        }


        Guid IGameCommands.WinBet([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = GetRound(actionData);

                var walletTransactionId = _walletOperations.WinBet(round.Data.PlayerId, round.Data.GameId, round.Data.Id, actionData.Amount);

                var winGameActionId = round.Win(actionData.Amount, actionData.Description, walletTransactionId, actionData.TokenId, actionData.ExternalTransactionId, actionData.BatchId);

                _repository.SaveChanges();


                _eventBus.Publish(new BetWon
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = winGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });

                scope.Complete();

                return winGameActionId;
            }
        }

        Guid IGameCommands.LoseBet([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            if (actionData.Amount != 0)
            {
                throw new LoseBetAmountMustBeZeroException();
            }
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = GetRound(actionData);

                _walletOperations.LoseBet(round.Data.PlayerId, round.Data.GameId, round.Data.Id);

                var loseGameActionId = round.Lose(actionData.Description, actionData.TokenId, actionData.ExternalTransactionId, actionData.BatchId);

                _repository.SaveChanges();

                _eventBus.Publish(new BetLost
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = loseGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });

                scope.Complete();

                return loseGameActionId;
            }
        }


        async Task<Guid> IGameCommands.WinBetAsync([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(actionData);

                var walletTransactionId = await _walletOperations.WinBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id, actionData.Amount);

                var winGameActionId = round.Win(actionData.Amount, actionData.Description, walletTransactionId, actionData.TokenId, actionData.ExternalTransactionId, actionData.BatchId);

                await _repository.SaveChangesAsync();


                _eventBus.Publish(new BetWon
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = winGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });

                scope.Complete();

                return winGameActionId;
            }
        }


        async Task<Guid> IGameCommands.LoseBetAsync([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            if (actionData.Amount != 0)
            {
                throw new LoseBetAmountMustBeZeroException();
            }
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(actionData);

                _walletOperations.LoseBet(round.Data.PlayerId, round.Data.GameId, round.Data.Id);

                var loseGameActionId = round.Lose(actionData.Description, actionData.TokenId, actionData.ExternalTransactionId, actionData.BatchId);

                await _repository.SaveChangesAsync();

                _eventBus.Publish(new BetLost
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = loseGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });

                scope.Complete();

                return loseGameActionId;
            }
        }

        
        //
        // The idea of the "Free bet" is that game provider can let players win a bet without actually placing it
        //
        Guid IGameCommands.FreeBet([NotNull]GameActionData actionData, [NotNull]GameActionContext context, [NotNull]TokenData token)
        {
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = _repository.GetOrCreateRound(actionData.RoundId, token);

                var walletTransactionId = _walletOperations.FreeBet(round.Data.PlayerId, round.Data.GameId, actionData.Amount);

                var freeBetGameActionId = round.Free(actionData.Amount, actionData.Description, walletTransactionId, actionData.TokenId, actionData.ExternalTransactionId);

                _repository.Rounds.AddOrUpdate(x => x.ExternalRoundId, round.Data);

                _repository.SaveChanges();

                _eventBus.Publish(new BetPlacedFree
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = freeBetGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });

                scope.Complete();

                return freeBetGameActionId;
            }
        }

        Guid IGameCommands.AdjustTransaction([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = GetRound(actionData);

                var gameActionToAdjust = round.GetGameActionByReferenceId(actionData.TransactionReferenceId);

                var walletTransactionId = _walletOperations.AdjustBetTransaction(round.Data.PlayerId, round.Data.GameId,
                    gameActionToAdjust.Id, actionData.Amount);

                var adjustmentGameActionId = round.Adjust(actionData.Amount, actionData.Description, walletTransactionId, actionData.TokenId, actionData.ExternalTransactionId,
                    actionData.TransactionReferenceId, actionData.BatchId);

                _repository.SaveChanges();

                _eventBus.Publish(new BetAdjusted
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = adjustmentGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });

                scope.Complete();

                return adjustmentGameActionId;
            }
        }

        Guid IGameCommands.CancelTransaction([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, context.GameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = GetRound(actionData);

                var gameActionToCancel = round.GetGameActionByReferenceId(actionData.TransactionReferenceId);
                var walletTransactionId = _walletOperations.CancelBet(round.Data.PlayerId, round.Data.GameId, gameActionToCancel.WalletTransactionId);

                var amount = -gameActionToCancel.Amount;
                if (gameActionToCancel.Amount != amount)
                {
                    // TODO: raise an administrative event (amounts don't match when cancelling a transaction)   
                }

                var cancelGameActionId = round.Cancel(amount, actionData.Description, walletTransactionId, actionData.ExternalTransactionId,
                    actionData.TransactionReferenceId, actionData.TokenId, actionData.BatchId);

                _repository.SaveChanges();

                _eventBus.Publish(new BetCancelled
                {
                    PlayerId = round.Data.PlayerId,
                    BrandId = round.Data.BrandId,
                    GameId = round.Data.GameId,
                    RoundId = round.Data.Id,
                    GameActionId = cancelGameActionId,
                    CreatedOn = round.Data.CreatedOn,
                    Amount = round.Amount,
                    AdjustedAmount = round.AdjustedAmount,
                    WonAmount = round.WonAmount
                });

                scope.Complete();

                return cancelGameActionId;
            }
        }

        private Round GetRound(GameActionData actionData)
        {
            var round = _repository.GetRound(actionData.RoundId);
            if (round == null)
                throw new RoundNotFoundException();
            return round;
        }


        // private

        private void ValidateTransactionIsUnique(string transactionId, Guid gameProviderId)
        {
            if (_repository.DoesGameActionExist(transactionId, gameProviderId))
            {
                var dupTx = FindGameAction(transactionId, gameProviderId);
                throw new DuplicateGameActionException(dupTx.Id);
            }
        }


        private GameAction FindGameAction(string externalTransactionId, Guid gameProviderId)
        {
            return _repository.GetGameActionByExternalTransactionId(externalTransactionId, gameProviderId);
        }
    }
}