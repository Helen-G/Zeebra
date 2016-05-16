using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Extensions;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Tests.Core;
using AFT.RegoV2.GameApi.Tests.Integration.Steps.Entities;
using FluentAssertions;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace AFT.RegoV2.GameApi.Tests.Integration.Steps
{
    [Binding, Scope(Feature = "ACS Soleil Game Api")]
    public class ACSSoleilGameApi : SpecFlowIntegrationTestBase
    {
        [Given(@"I get authentication token for player ""(.*)"" with password ""(.*)"" for game ""(.*)""")]
        public async Task GivenIGetAuthenticationTokenForPlayer(string player, string password, string game)
        {
            var token = await GetTokenFor(player, password, game);

            Set(SR.token, token);

            token.Should().NotBeNullOrEmpty();
        }

        [Given(@"I get authorization token for game provider ""(.*)"" with secret ""(.*)""")]
        public async Task GivenIGetAuthorizationTokenForPlayer(string clientId, string clientSecret)
        {
            var accessToken = await GetAccessTokenFor(clientId, clientSecret);

            Set(SR.accesstoken, accessToken);

            accessToken.Should().NotBeNullOrEmpty();
        }

        [When(@"I call to validate token")]
        public async Task WhenICallToValidateToken()
        {
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);
            var response =
                await JsonPostSecure<ValidateTokenResponse>(
                    Config.GameApiUrl + "api/soleil/token/validate",
                    accessToken,
                    new ValidateToken
                    {
                        PlayerIpAddress = DefaultPlayertIp,
                        AuthToken = token
                    });

            Set(SR.response, response);
        }
        [Then(@"I will receive successful validation result")]
        public void ThenIWillReceiveSuccessfulValidationResult()
        {
            var response = Get<ValidateTokenResponse>(SR.response);

            response.Should().NotBeNull();
            response.ErrorCode.Should().Be(GameApiErrorCode.NoError);
            response.PlayerDisplayName.Should().NotBeNull();
            response.PlayerId.Should().NotBe(Guid.Empty);
        }
        [Given(@"I validate the token")]
        public async Task GivenIValidateTheToken()
        {
            await WhenICallToValidateToken();
            ThenIWillReceiveSuccessfulValidationResult();
        }
        [Given(@"the player ""(.*)"" main balance is \$(.*) and bonus balance is \$(.*)")]
        public async Task GivenThePlayerMainBalanceIsAndBonusBalanceIs(string player, decimal mainBalance, decimal bonusBalance)
        {
            var gsiDb = GetOrCreateGamesDb();
            var walletDb = gsiDb;
            var gameProviderId = Get<Guid>(SR.GameProviderId);

            var playerId = await gsiDb.Players.Where(p => p.Name == player).Select(p => p.Id).FirstOrDefaultAsync();
            var wallet = await (from w in walletDb.Wallets
                join wtgp in walletDb.WalletTemplateGameProviders on w.Template.Id equals wtgp.WalletTemplateId
                join gp in walletDb.GameProviders on wtgp.GameProviderId equals gp.Id
                where w.PlayerId == playerId && gp.Id == gameProviderId
                select w)
                .Include(x => x.Brand)
                .Include(x => x.Template)
                .SingleAsync();

            wallet.Main = mainBalance;
            wallet.Bonus = bonusBalance;

            await walletDb.SaveChangesAsync();
        }
        [When(@"I bet \$(.*)")]
        public async Task WhenIBet(decimal amount)
        {
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);

            var betIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var response =
                await JsonPostSecure<PlaceBetResponse>(Config.GameApiUrl + "api/soleil/bets/place", 
                        accessToken,
                        new PlaceBet
                        {
                            AuthToken = token,
                            Transactions = new List<PlaceBetTransaction>
                            {
                                new PlaceBetTransaction
                                {
                                    Id = Set(SR.transactionId, NewStringId),
                                    RoundId = betIdsList.AddAndPass(Set(SR.roundId, NewStringId)),
                                    CurrencyCode = "CNY",
                                    TimeStamp = DateTimeOffset.UtcNow,
                                    Amount = amount
                                }
                            }
                        });

            Set(SR.response, response);
        }
        [When(@"I win \$(.*)")]
        public async Task WhenIWin(decimal amount)
        {
            var accessToken = Get<string>(SR.accesstoken);
            var token = Get<string>(SR.token);
            var roundId = Get<string>(SR.roundId);
            var response =
                await JsonPostSecure<PlaceBetResponse>(Config.GameApiUrl + "api/soleil/bets/win", 
                    accessToken,
                    new WinBet
                    {
                        AuthToken = token,
                        Transactions = new List<SettleBetTransaction>
                        {
                            new SettleBetTransaction
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                RoundId = roundId,
                                CurrencyCode = "CNY",
                                TimeStamp = DateTimeOffset.UtcNow,
                                Amount = amount
                            }
                        }
                    });
            Set(SR.response, response);
        }
        [When(@"I lose the bet")]
        public async Task WhenILoseTheBet()
        {
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);

            var roundId = Get<string>(SR.roundId);
            var response =
                await JsonPostSecure<LoseBetResponse>(Config.GameApiUrl + "api/soleil/bets/lose", 
                    accessToken,
                    new LoseBet
                    {
                        AuthToken = token,
                        Transactions = new List<SettleBetTransaction>
                        {
                            new SettleBetTransaction
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                RoundId = roundId,
                                CurrencyCode = "CNY",
                                TimeStamp = DateTimeOffset.UtcNow,
                                Amount = 0 // MUST BE 0
                            }
                        }
                    });
            Set(SR.response, response);
        }
        [When(@"I get free bet for \$(.*)")]
        public async Task WhenIGetFreeBetFor(decimal amount)
        {
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);

            var roundIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var response =
                await JsonPostSecure<FreeBetResponse>(Config.GameApiUrl + "api/soleil/bets/freebet", 
                    accessToken,
                    new FreeBet
                    {
                        AuthToken = token,
                        Transactions = new List<SettleBetTransaction>
                        {
                            new SettleBetTransaction
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                RoundId = roundIdsList.AddAndPass(Set(SR.roundId, NewStringId)),
                                CurrencyCode = "CNY",
                                TimeStamp = DateTimeOffset.UtcNow,
                                Amount = amount
                            }
                        }
                    });
            Set(SR.response, response);
        }
        [Then(@"requested bet (.*) recorded")]
        public void ThenRequestedBetillNotBeRecorded(bool willBeRecorded)
        {
            ThenRequestedBetsWillNotBeRecorded(willBeRecorded);
        }
        [When(@"I place bets for amount:")]
        public async Task WhenIPlaceBetsForAmount(Table table)
        {
            var bets = table.CreateSet(tr => Decimal.Parse(tr.Values.First()));

            foreach (var amount in bets)
            {
                await WhenIBet(amount);
            }
        }
        [When(@"I settle the following bets:")]
        public async Task WhenISettleTheFollowingBets(Table table)
        {
            var accessToken = Get<string>(SR.accesstoken);

            var rounds = table.CreateSet<TypeAndAmount>();

            var roundIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var i = 0;
            var response =
                await JsonPostSecure<SettleBetsResponse>(Config.GameApiUrl + "api/soleil/batch/bets/settle", 
                    accessToken,
                    new SettleBets
                    {
                        BatchId = NewStringId,
                        BrandKey = "138",
                        Transactions = 
                            rounds.Select(taa => new BatchSettleBetTransaction
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                RoundId = roundIdsList[i++],
                                CurrencyCode = "CNY",
                                TimeStamp = DateTimeOffset.UtcNow,
                                Amount = taa.Amount,
                                BrandCode = "138",
                                TransactionType = 
                                    taa.Type == "WIN" 
                                        ? BatchSettleBetTransactionType.Win 
                                        : BatchSettleBetTransactionType.Lose
                            })
                            .ToList()
                    });
            Set(SR.response, response);
        }
        [Then(@"requested bets (.*) recorded")]
        public void ThenRequestedBetsWillNotBeRecorded(bool willBeRecorded)
        {
            var gsiDb = GetOrCreateGamesDb();
            var roundIdsList = Get<List<string>>(SR.roundIdsList);
            if(roundIdsList == null) Assert.Fail("Expected to have placed bet(s)");
            foreach(var roundId in roundIdsList)
            {
                var exists = gsiDb.Rounds.Any(b => b.ExternalRoundId == roundId);
                if (willBeRecorded != exists)
                {
                    Assert.Fail("Expected bet with ID={0} was {1}found".Args(roundId, willBeRecorded ? "not " : ""));
                }
            };
        }
        [Then(@"place bet response balance will equal requested balance")]
        public void ThenPlaceBetResponseBalanceWillEqualRequestedBalance()
        {
            var response = Get<PlaceBetResponse>(SR.response);
            var balance = Get<GetBalanceResponse>(SR.balance);

            response.Balance.Should().Be(balance.Balance);
        }
        [Then(@"I will get error code ""(.*)""")]
        public void ThenIWillGetErrorCode(GameApiErrorCode error)
        {
            var response = Get<GameApiResponseBase>(SR.response);

            response.ErrorCode.Should().Be(error);
        }
        [Then(@"the player's playable balance will be \$(.*)")]
        public void ThenThePlayersPlayableBalanceWillBe(decimal playableBalance)
        {
            var balance = Get<GetBalanceResponse>(SR.balance);

            balance.Balance.Should().Be(playableBalance);
        }
        [When(@"I get balance")]
        public async Task WhenIGetBalance()
        {
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);

            var balance =
                await JsonGetSecure<GetBalanceResponse>(Config.GameApiUrl + "api/soleil/players/balance?authtoken=" + token, accessToken);

            Set(SR.balance, balance);
        }
        [When(@"I get history")]
        public async Task WhenIGetHistory()
        {
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);

            var history =
                await JsonGetSecure<BetsHistoryResponse>(Config.GameApiUrl + "api/soleil/bets/history?authtoken=" + token, accessToken);

            Set(SR.response, history);
        }
        [Then(@"I will see the bet IDs in the history")]
        public void ThenIWillSeeTheBetIDsInTheHistory()
        {
            var history = Get<BetsHistoryResponse>(SR.response);
            var betIdsList = Get<List<string>>(SR.roundIdsList);
            if(betIdsList == null || betIdsList.Count == 0) Assert.Fail("Expected to have placed one or more bet(s)");

            history.Should().NotBeNull();
            foreach (var roundId in betIdsList)
            {
                if (history.Rounds.All(b => b.Id != roundId))
                {
                    Assert.Fail("Expected to see bet with ID=" + roundId);
                }
            }
        }
        [When(@"I adjust transaction with \$(.*)")]
        public async Task WhenIAdjustTransactionWith(decimal amount)
        {
            var roundId = Get<string>(SR.roundId);
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);

            var response =
                await JsonPostSecure<AdjustTransactionResponse>(Config.GameApiUrl + "api/soleil/transactions/adjust", 
                    accessToken,
                    new AdjustTransaction
                    {
                        AuthToken = token,
                        Transactions = 
                            new List<AdjustTransactionData>
                            {
                                new AdjustTransactionData
                                {
                                    Id = Set(SR.transactionId, NewStringId),
                                    RoundId = roundId,
                                    Amount = amount,
                                    CurrencyCode = "CNY",
                                    TimeStamp = DateTimeOffset.UtcNow
                                }
                            }
                    });
            Set(SR.response, response);
        }
        [When(@"I cancel the last transaction")]
        public async Task WhenICancelTheLastTransaction()
        {
            var roundId = Get<string>(SR.roundId);
            var token = Get<string>(SR.token);
            var accessToken = Get<string>(SR.accesstoken);
            var transactionId = Get<string>(SR.transactionId);

            var response =
                await JsonPostSecure<CancelTransactionResponse>(Config.GameApiUrl + "api/soleil/transactions/cancel", 
                    accessToken,
                    new CancelTransaction
                    {
                        AuthToken = token,
                        Transactions = 
                            new List<CancelTransactionData>
                            {
                                new CancelTransactionData
                                {
                                    Id = NewStringId,
                                    RoundId = roundId,
                                    ReferenceId = transactionId,
                                    TimeStamp = DateTimeOffset.UtcNow
                                }
                            }
                    });
            Set(SR.response, response);
        }
    }
}