﻿using System;
﻿using System.Web.Http;
﻿using AFT.RegoV2.Core.Common.Data;
﻿using AFT.RegoV2.Core.Common.Interfaces;
﻿using AFT.RegoV2.Core.Game.ApplicationServices;
﻿using AFT.RegoV2.Core.Game.Data;
﻿using AFT.RegoV2.Core.Game.Exceptions;
﻿using AFT.RegoV2.Core.Game.Interfaces;
﻿using AFT.RegoV2.GameApi.Interface.ServiceContracts.SlotServer;
﻿using AFT.RegoV2.Infrastructure.Providers;

namespace AFT.RegoV2.GameApi.SlotServer.Controllers
{

    //[EnsureSimpleXmlSerialization]
    public class SlotServerController : ApiController
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly IGameQueries _gameQueries;
        private readonly IGameCommands _gameCommands;

        public SlotServerController(
            ITokenProvider tokenProvider, 
            IGameQueries gameQueries,
            IGameCommands gameCommands)
        {
            _tokenProvider = tokenProvider;
            _gameQueries = gameQueries;
            _gameCommands = gameCommands;
        }

        [Route("api/slotserver/validateToken")]
        public ValidateTokenResponse Post(ValidateToken request)
        {
            var ipAddress = request.IpAddress ?? "::1";

            try
            {
                var token = _tokenProvider.Decrypt(request.Token);

                var player = _gameQueries.GetPlayerData(token.PlayerId);

                return new ValidateTokenResponse
                {
                    UserName = player.Name,
                    StatusCode = "100",
                    Currency = "",
                    MemberCode = player.Id.ToString(),
                    IpAddress = ipAddress,
                    StatusDesc = "",
                    CashierUrl = "",
                    CustomerSupportUrl = "",
                    LobbyUrl = "",
                    Result = true

                };

            }
            catch (InvalidTokenException)
            {
                return new ValidateTokenResponse
                {
                    Result = false,
                    UserName = "",
                    StatusCode = "101",
                    Currency = "",
                    MemberCode = "",
                    IpAddress = "",
                    StatusDesc = "Invalid token.",
                    CashierUrl = "",
                    CustomerSupportUrl = "",
                    LobbyUrl = "",
                };
            }
        }

        [Route("api/slotserver/getbalance")]
        [Route("api/slotserver/fundtransfer")]
        public WalletOperationResponse Post(WalletOperation request)
        {
            switch (request.Operation)
            {
                case "GetBalance":
                    return GetPlayerBalance(request);
                case "FundTransfer":
                    return FundTransfer(request);
            }
            throw new NotImplementedException();
        }

        private const string brandIdString = "00000000-0000-0000-0000-000000000138";

        private WalletOperationResponse FundTransfer(WalletOperation request)
        {
            var playerId = Guid.Parse(request.MemberCode);

            var roundId = request.HandId;
            var subType = Convert.ToInt32(request.TransactionSubTypeId);
            var token = new TokenData
            {
                GameId = Guid.Empty,
                PlayerId = playerId,
                PlayerIpAddress = "::1",
                BrandId = new Guid(brandIdString)
            };
            try
            {
                switch (subType)
                {
                    case 500:
                        _gameCommands.PlaceBet(
                            new GameActionData
                            {
                                Amount = -request.Amount,
                                RoundId = roundId,
                                ExternalTransactionId = request.TransactionId
                            },
                            new GameActionContext(), token);
                        break;
                    case 510:
                        _gameCommands.WinBet(
                            new GameActionData
                            {
                                Amount = request.Amount,
                                RoundId = roundId,
                                ExternalTransactionId = request.TransactionId
                            },
                            new GameActionContext());
                        break;
                    case 520:
                        _gameCommands.LoseBet(
                            new GameActionData
                            {
                                Amount = 0,
                                RoundId = roundId,
                                ExternalTransactionId = request.TransactionId
                            },
                            new GameActionContext());
                        break;
                }
            }
            catch (InvalidAmountException)
            {
                return new WalletOperationResponse
                {
                    Error = OureaErrors.InvalidAmount.ToString(),
                    Amount = _gameQueries.GetPlayableBalance(playerId, token.GameId)
                };

            }
            catch (InsufficientFundsException)
            {
                return new WalletOperationResponse
                {
                    Error = OureaErrors.InsufficientBalance.ToString(),
                    Amount = _gameQueries.GetPlayableBalance(playerId, token.GameId)
                };
            }
            return new WalletOperationResponse
            {
                Error = "0",
                Amount = _gameQueries.GetPlayableBalance(playerId, token.GameId)
            };
        }

        private WalletOperationResponse GetPlayerBalance(WalletOperation request)
        {
            var playerId = Guid.Parse(request.MemberCode);

            var errorCode = "0";
            var amount = 0m;

            if (_gameQueries.DoesPlayerIdExist(playerId) == false)
            {
                errorCode = OureaErrors.AccountNotFound.ToString();
            }

            amount = _gameQueries.GetPlayableBalance(playerId, new Guid(brandIdString));

            return new WalletOperationResponse
            {
                Error = errorCode,
                Amount = amount
            };
        }

    }

    public enum OureaErrors
    {
        NoError = 0,
        AccountNotFound = 1000,
        InvalidCurrency = 1001,
        InvalidAmount = 1002,
        LockedAccount = 1004,
        InsufficientBalance = 9999
    }
}
