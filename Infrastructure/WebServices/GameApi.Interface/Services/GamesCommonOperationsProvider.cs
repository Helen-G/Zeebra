using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Exceptions;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.Infrastructure.Providers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.GameApi.Interface.Services
{
    public interface IGamesCommonOperationsProvider
    {
        ValidateTokenResponse ValidateToken(ValidateToken request);
        Task<ValidateTokenResponse> ValidateTokenAsync(ValidateToken validateToken);
        GetBalanceResponse GetBalance(GetBalance request);
        Task<PlaceBetResponse> PlaceBet(PlaceBet request);
        WinBetResponse WinBet(WinBet request);
        LoseBetResponse LoseBet(LoseBet request);
        FreeBetResponse FreeBet(FreeBet request);
        AdjustTransactionResponse AdjustTransaction(AdjustTransaction request);
        CancelTransactionResponse CancelTransaction(CancelTransaction request);
        Task<SettleBetsResponse> SettleBets(SettleBets request);
        AdjustTransactionsResponse AdjustTransactions(AdjustTransactions request);
        CancelTransactionsResponse CancelTransactions(CancelTransactions request);
        BetsHistoryResponse GetBetHistory(RoundsHistory request);
        Task<decimal> GetBalanceAsync(GetBalance request);
    }
    public sealed class GamesCommonOperationsProvider : IGamesCommonOperationsProvider
    {
        [Dependency]
        internal ITokenProvider TokenProvider { get; set; }

        private readonly IUnityContainer _container;
        private readonly IGameCommands _gameCommands;
        private readonly IGameQueries _gameQueries;
        private readonly ITransactionScopeProvider _transactionScope;
        private readonly IErrorManager _errors;

        public GamesCommonOperationsProvider(
            IUnityContainer container,
            IGameCommands gameCommands, 
            IGameQueries gameQueries,
            ITransactionScopeProvider transactionScope,
            IErrorManager errors)
        {
            _container = container;
            _gameCommands = gameCommands;
            _gameQueries = gameQueries;
            _transactionScope = transactionScope;
            _errors = errors;
        }

        ValidateTokenResponse IGamesCommonOperationsProvider.ValidateToken(ValidateToken request)
        {
            var tokenData = GetTokenData(request);
            var playerId = tokenData.PlayerId;

            var playerData = _gameQueries.GetPlayerData(playerId); // Get player

            if (playerData == null)
                throw new PlayerNotFoundException(String.Format("Player not found. {0}", playerId));

            var brand = _gameQueries.GetBrand(tokenData.BrandId) ?? new Brand { Code = "" }; // Get brand
            var game = _gameQueries.GetGameDto(tokenData.GameId); // Get game

            var balance = _gameQueries.GetPlayableBalance(playerId, tokenData.GameId); // Get balance

            var playerBetLimitCode = _gameQueries.GetPlayerBetLimitCodeOrNull(playerData.VipLevelId, game.ProductId,
                    tokenData.CurrencyCode) ?? "";

            return new ValidateTokenResponse
            {
                PlayerId = playerData.Id,
                PlayerDisplayName = playerData.Name,
                BrandCode = brand.Code,
                Language = playerData.CultureCode ?? string.Empty,
                CurrencyCode = playerData.CurrencyCode ?? string.Empty,
                Balance = balance,
                BetLimitCode = playerBetLimitCode
            };
        }

        async Task<ValidateTokenResponse> IGamesCommonOperationsProvider.ValidateTokenAsync(ValidateToken request)
        {
            var tokenData = GetTokenData(request);
            var playerId = tokenData.PlayerId;

            var playerData = await _gameQueries.GetPlayerDataAsync(playerId); // Get player

            if (playerData == null)
                throw new PlayerNotFoundException("Cannot find player with id=" + playerId);

            var balance = await _gameQueries.GetPlayerBalanceAsync(playerId, tokenData.GameId); // Get balance
            var brandCode = (await _gameQueries.GetBrandCodeAsync(tokenData.BrandId)) ?? ""; // Get brand
            var gameProviderId = await _gameQueries.GetGameProviderIdByGameIdAsync(tokenData.GameId); // Get game

            var playerBetLimitCode = "";

            if (gameProviderId != Guid.Empty)
                playerBetLimitCode = await _gameQueries.GetPlayerBetLimitCodeOrNullAsync(playerData.VipLevelId, gameProviderId, tokenData.CurrencyCode);

            return new ValidateTokenResponse
            {
                PlayerId = playerData.Id,
                PlayerDisplayName = playerData.Name,
                BrandCode = brandCode,
                Language = playerData.CultureCode ?? string.Empty,
                CurrencyCode = playerData.Currency.Code ?? string.Empty,
                Balance = balance,
                BetLimitCode = playerBetLimitCode ?? ""
            };
        }

        GetBalanceResponse IGamesCommonOperationsProvider.GetBalance(GetBalance request)
        {
            var tokenData = GetTokenData(request);
            var balance = _gameQueries.GetPlayableBalance(tokenData.PlayerId, tokenData.GameId);
            return new GetBalanceResponse
            {
                Balance = balance,
                CurrencyCode = tokenData.CurrencyCode,
            };
        }

        Task<decimal> IGamesCommonOperationsProvider.GetBalanceAsync(GetBalance request)
        {
            var tokenData = GetTokenData(request);

            return _gameQueries.GetPlayerBalanceAsync(tokenData.PlayerId, tokenData.GameId);
        }

        async Task<PlaceBetResponse> IGamesCommonOperationsProvider.PlaceBet(PlaceBet request)
        {
            using (var scope = _transactionScope.GetTransactionScopeAsync())
            {
                var tokenData = GetTokenData(request);

                var result = request.Transactions.Select(tx => PlaceBet(tokenData, tx)).ToList();
                var balance = await _gameQueries.GetPlayerBalanceAsync(tokenData.PlayerId, tokenData.GameId);

                scope.Complete();

                return new PlaceBetResponse
                {
                    Balance = balance,
                    CurrencyCode = tokenData.CurrencyCode,
                    Transactions = result
                };
            }
        }

        WinBetResponse IGamesCommonOperationsProvider.WinBet(WinBet request)
        {
            using (var scope = _transactionScope.GetTransactionScope())
            {
                var tokenData = GetTokenData(request);

                var result = request.Transactions.Select(tx =>
                    SettleBet(tokenData, tx, (data, context) =>
                        _gameCommands.WinBet(data, context))).ToList();

                var balance = _gameQueries.GetPlayableBalance(tokenData.PlayerId, tokenData.GameId);

                scope.Complete();

                return new WinBetResponse
                {
                    Balance = balance,
                    CurrencyCode = tokenData.CurrencyCode,
                    Transactions = result
                };
            }
        }

        LoseBetResponse IGamesCommonOperationsProvider.LoseBet(LoseBet request)
        {
            using (var scope = _transactionScope.GetTransactionScope())
            {
                var tokenData = GetTokenData(request);

                var result = request.Transactions.Select(tx =>
                    SettleBet(tokenData, tx, (data, context) =>
                        _gameCommands.LoseBet(data, context))).ToList();

                var balance = _gameQueries.GetPlayableBalance(tokenData.PlayerId, tokenData.GameId);

                scope.Complete();

                return new LoseBetResponse
                {
                    Balance = balance,
                    CurrencyCode = tokenData.CurrencyCode,
                    Transactions = result
                };
            }
        }

        FreeBetResponse IGamesCommonOperationsProvider.FreeBet(FreeBet request)
        {
            using (var scope = _transactionScope.GetTransactionScope())
            {
                var tokenData = GetTokenData(request);
                var result = request.Transactions.Select(tx =>
                    SettleBet(tokenData, tx, (data, context) =>
                        _gameCommands.FreeBet(data, context, tokenData))).ToList();

                var balance = _gameQueries.GetPlayableBalance(tokenData.PlayerId, tokenData.GameId);

                scope.Complete();

                return new FreeBetResponse
                {
                    Balance = balance,
                    CurrencyCode = tokenData.CurrencyCode,
                    Transactions = result
                };
            }
        }

        AdjustTransactionResponse IGamesCommonOperationsProvider.AdjustTransaction(AdjustTransaction request)
        {
            using (var scope = _transactionScope.GetTransactionScope())
            {
                var tokenData = GetTokenData(request);

                var result = request.Transactions.Select(tx => AdjustTransaction(tokenData, tx)).ToList();

                var balance = _gameQueries.GetPlayableBalance(tokenData.PlayerId, tokenData.GameId);

                scope.Complete();

                return new AdjustTransactionResponse
                {
                    Balance = balance,
                    CurrencyCode = tokenData.CurrencyCode,
                    Transactions = result
                };
            }
        }

        CancelTransactionResponse IGamesCommonOperationsProvider.CancelTransaction(CancelTransaction request)
        {
            using (var scope = _transactionScope.GetTransactionScope())
            {
                var tokenData = GetTokenData(request);

                var result = request.Transactions.Select(tx => CancelTransaction(tokenData, tx)).ToList();

                var balance = _gameQueries.GetPlayableBalance(tokenData.PlayerId, tokenData.GameId);

                scope.Complete();

                return new CancelTransactionResponse
                {
                    Balance = balance,
                    CurrencyCode = tokenData.CurrencyCode,
                    Transactions = result
                };
            }
        }

        async Task<SettleBetsResponse> IGamesCommonOperationsProvider.SettleBets(SettleBets request)
        {
            
            var errorCode = GameApiErrorCode.NoError;
            var errorDescription = (string) null;
            var errorsList = new ConcurrentBag<BatchTransactionError>();
            var txCount = 0;
            var isDuplicateBatch = 0;
            var timer = new Stopwatch();

            timer.Start();
            try
            {
                _gameQueries.ValidateBatchIsUnique(request.BatchId, GameProviderId);

                txCount = await SettleBetTransactionsAsync(request, errorsList);
            }
            catch (DuplicateBatchException ex)
            {
                isDuplicateBatch = 1;
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }
            catch (Exception ex)
            {
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }

            timer.Stop();

            return new SettleBetsResponse
            {
                BatchId = request.BatchId,
                TransactionCount = txCount,
                BatchTimestamp = DateTimeOffset.UtcNow.ToString("O"),
                Elapsed = timer.ElapsedMilliseconds,
                Errors = errorsList.ToList(),
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
                IsDuplicate = isDuplicateBatch
            };
        }

        private async Task<int> SettleBetTransactionsAsync(SettleBets request,ConcurrentBag<BatchTransactionError> errorsList)
        {

            var brands = new ConcurrentDictionary<string, Brand>();

            int txCount = 0;

            var userGroups = request.Transactions.GroupBy(x => x.UserId);

            await Task.WhenAll(
                userGroups.Select(userTransactions =>
                    Task.Run(async () =>
                    {
                        var commands = _container.Resolve<IGameCommands>();

                        foreach (var transaction in userTransactions)
                        {
                            try
                            {
                                var betData = new GameActionData
                                {
                                    RoundId = transaction.RoundId,
                                    ExternalTransactionId = transaction.Id,
                                    Amount = transaction.Amount,
                                    CurrencyCode = transaction.CurrencyCode,
                                    Description = transaction.Description,
                                    BatchId = request.BatchId,
                                    TokenId = GetTokenData(transaction, brands).TokenId
                                };

                                var GameActionContext = new GameActionContext
                                {
                                    GameProviderId = GameProviderId
                                };


                                switch (transaction.TransactionType)
                                {
                                    case BatchSettleBetTransactionType.Win:
                                        await commands.WinBetAsync(betData, GameActionContext);
                                        break;
                                    case BatchSettleBetTransactionType.Lose:
                                        await commands.LoseBetAsync(betData, GameActionContext);
                                        break;
                                }
                                Interlocked.Increment(ref txCount);
                            }
                            catch (Exception ex)
                            {
                                errorsList.Add(CreateBatchTransactionError(ex, transaction.Id, transaction.UserId));
                            }
                        }
                    })));
            return txCount;
        }

        private TokenData GetTokenData(BatchSettleBetTransaction transaction, ConcurrentDictionary<string, Brand> brands)
        {
            var brandCode = transaction.BrandCode;
            Brand brand;
            if (brands.ContainsKey(brandCode))
            {
                brand = brands[brandCode];
            }
            else
            {
                var gamesQueries = _container.Resolve<IGameQueries>();
                brand = gamesQueries.GetBrand(transaction.BrandCode) ?? new Brand {Code = ""};
                brands.TryAdd(brandCode, brand);
            }

            var token = new TokenData
            {
                BrandId = brand.Id,
                GameId = Guid.Empty, // TODO
                PlayerId = transaction.UserId,
                CurrencyCode = transaction.CurrencyCode
            };
            return token;
        }

        AdjustTransactionsResponse IGamesCommonOperationsProvider.AdjustTransactions(AdjustTransactions request)
        {
            var errorCode = GameApiErrorCode.NoError;
            var errorDescription = (string) null;
            var errorsList = new ConcurrentBag<BatchTransactionError>();
            var txCount = 0;

            try
            {
                txCount = request.Transactions.Count;

                request.Transactions.AsParallel().ForAll(transaction =>
                {
//                    var brand = _gsiQueries.GetBrand(transaction.BrandCode) ?? new Brand { Code = "" };

                    // TODO: implement token functionality
                    var token = new TokenData
                    {
                        BrandId = Guid.Empty,
                        GameId = Guid.Empty, // TODO
                        PlayerId = transaction.UserId,
                        CurrencyCode = transaction.CurrencyCode,
                        TokenId = Guid.NewGuid()
                    };

                    try
                    {
                        AdjustTransaction(token, transaction, request.BatchId);
                    }
                    catch (Exception ex)
                    {
                        errorsList.Add(CreateBatchTransactionError(ex, transaction.Id, transaction.UserId));
                    }
                });
            }
            catch (Exception ex)
            {
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }

            return new AdjustTransactionsResponse
            {
                BatchId = request.BatchId,
                TransactionCount = txCount,
                BatchTimestamp = DateTimeOffset.UtcNow.ToString("O"),
                Errors = errorsList.ToList(),
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            };
        }

        CancelTransactionsResponse IGamesCommonOperationsProvider.CancelTransactions(CancelTransactions request)
        {
            var errorCode = GameApiErrorCode.NoError;
            var errorDescription = (string) null;
            var errorsList = new ConcurrentBag<BatchTransactionError>();
            var txCount = 0;

            try
            {
                txCount = request.Transactions.Count;

                request.Transactions.AsParallel().ForAll(transaction =>
                {
                    var brand = _gameQueries.GetBrand(transaction.BrandCode) ?? new Brand { Code = "" };

                    // TODO: implement token functionality
                    var token = new TokenData
                    {
                        BrandId = brand.Id,
                        GameId = Guid.Empty, // TODO
                        PlayerId = transaction.UserId,
                        TokenId = Guid.NewGuid()
                    };

                    try
                    {
                        CancelTransaction(token, transaction, request.BatchId);
                    }
                    catch (Exception ex)
                    {
                        errorsList.Add(CreateBatchTransactionError(ex, transaction.Id, transaction.UserId));
                    }
                });
            }
            catch (Exception ex)
            {
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }

            return new CancelTransactionsResponse
            {
                BatchId = request.BatchId,
                TransactionCount = txCount,
                BatchTimestamp = DateTimeOffset.UtcNow.ToString("O"),
                Errors = errorsList.ToList(),
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            };
        }

        BetsHistoryResponse IGamesCommonOperationsProvider.GetBetHistory(RoundsHistory request)
        {
            var tokenData = GetTokenData(request);

            var rounds = _gameQueries.GetRoundHistory(tokenData, request.RecordCount);

            var convertedRounds = rounds.Select(round => new RoundHistoryData
            {
                Id = round.Data.ExternalRoundId,
                Status = round.Data.Status.ToString(),
                Amount = round.Amount,
                WonAmount = round.WonAmount,
                AdjustedAmount = round.AdjustedAmount,
                CreatedOn = round.Data.CreatedOn,
                ClosedOn = round.Data.ClosedOn,
                GameActions = round.Data.GameActions.Select(x => new GameActionHistoryData
                {
                    PlatformTxId = x.Id,
                    Amount = x.Amount,
                    Description = x.Description,
                    TransactionType = x.GameActionType.ToString(),
                    CreatedOn = x.CreatedOn,
                    Id = x.ExternalTransactionId,
                    TokenId = x.TokenId
                }).ToList()
            }).ToList();

            return new BetsHistoryResponse
            {
                Rounds = convertedRounds
            };
        }

        private BatchTransactionError CreateBatchTransactionError(Exception exception, string transactionId, Guid userId)
        {
            var betTxId = Guid.Empty;
            var duplicate = exception as DuplicateGameActionException;
            var isDuplicate = duplicate != null;

            if (isDuplicate)
                betTxId = duplicate.GameActionId;

            string errorDescription;
            var errorCode = _errors.GetErrorCodeByException(exception, out errorDescription);

            var error = new BatchTransactionError
            {
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
                GameActionId = betTxId,
                Id = transactionId,
                IsDuplicate = isDuplicate ? 1 : 0,
                UserId = userId
            };

            return error;
        }

        private TokenData GetTokenData(IGameApiRequest request)
        {
            return TokenProvider.Decrypt(request.AuthToken);
        }

     
        #region Bet methods
        private Guid GameProviderId
        {
            get { return Guid.Empty; }
        }
        private PlaceBetResponseTransaction PlaceBet(TokenData token, PlaceBetTransaction transaction)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {
                gameActionId = _gameCommands.PlaceBet(
                    new GameActionData
                    {
                        RoundId = transaction.RoundId,
                        ExternalTransactionId = transaction.Id,
                        Amount = transaction.Amount,
                        CurrencyCode = transaction.CurrencyCode,
                        Description = transaction.Description,
                        TokenId = token.TokenId
                    },
                    new GameActionContext
                    {
                        GameProviderId = GameProviderId
                    },
                    token);
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new PlaceBetResponseTransaction
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        private SettleBetResponseTransaction SettleBet(TokenData token, SettleBetTransaction transaction, Func<GameActionData, GameActionContext, Guid> settleMethod, string batchId = null)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {

                gameActionId = settleMethod(
                    new GameActionData
                    {
                        RoundId = transaction.RoundId,
                        ExternalTransactionId = transaction.Id,
                        Amount = transaction.Amount,
                        CurrencyCode = transaction.CurrencyCode,
                        Description = transaction.Description,
                        BatchId = batchId,
                        TokenId = token.TokenId
                    },
                    new GameActionContext
                    {
                        GameProviderId = GameProviderId
                    });
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new SettleBetResponseTransaction
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        private async Task<SettleBetResponseTransaction> SettleBetAsync(TokenData token, SettleBetTransaction transaction, Func<GameActionData, GameActionContext, Task<Guid>> settleMethod, string batchId = null)
        {
            var isDuplicate = 0;
            Guid gameActionId;
            
            try
            {
                var betData = new GameActionData
                {
                    RoundId = transaction.RoundId,
                    ExternalTransactionId = transaction.Id,
                    Amount = transaction.Amount,
                    CurrencyCode = transaction.CurrencyCode,
                    Description = transaction.Description,
                    BatchId = batchId,
                    TokenId = token.TokenId
                };

                var GameActionContext = new GameActionContext
                {
                    GameProviderId = GameProviderId
                };

                gameActionId = await settleMethod(betData, GameActionContext);
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new SettleBetResponseTransaction
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        private AdjustTransactionDataResponse AdjustTransaction(TokenData token, AdjustTransactionDataBase transaction, string batchId = null)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {
                gameActionId = _gameCommands.AdjustTransaction(
                    new GameActionData
                    {
                        RoundId = transaction.RoundId,
                        ExternalTransactionId = transaction.Id,
                        TransactionReferenceId = transaction.ReferenceId,
                        Amount = transaction.Amount,
                        CurrencyCode = transaction.CurrencyCode,
                        Description = transaction.Description,
                        BatchId = batchId,
                        TokenId = token.TokenId
                    },
                    new GameActionContext
                    {
                        GameProviderId = GameProviderId
                    });
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new AdjustTransactionDataResponse
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        private CancelTransactionDataResponse CancelTransaction(TokenData token, CancelTransactionDataBase transaction, string batchId = null)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {
                gameActionId = _gameCommands.CancelTransaction(
                    new GameActionData
                    {
                        RoundId = transaction.RoundId,
                        ExternalTransactionId = transaction.Id,
                        Amount = 0,
                        TransactionReferenceId = transaction.ReferenceId,
                        Description = transaction.Description,
                        BatchId = batchId,
                        TokenId = token.TokenId,
                    },
                    new GameActionContext
                    {
                        GameProviderId = GameProviderId
                    });
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new CancelTransactionDataResponse
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }
        #endregion
    }
}