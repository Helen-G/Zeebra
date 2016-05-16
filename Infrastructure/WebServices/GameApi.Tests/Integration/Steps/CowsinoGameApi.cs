using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
    [Binding, Scope(Feature = "Cowsino Game Api")]
    public class CowsinoGameApi : SpecFlowIntegrationTestBase
    {
        [Given(@"I get authentication token for player ""(.*)"" with password ""(.*)"" for game ""(.*)""")]
        public async Task GivenIGetAuthenticationTokenForPlayer(string player, string password, string game)
        {
            var token = await GetTokenFor(player, password, game);

            Set(SR.token, token);

            token.Should().NotBeNullOrEmpty();
        }
        [When(@"I call to validate token")]
        public async Task WhenICallToValidateToken()
        {
            var token = Get<string>(SR.token);
            var response =
                await JsonPost<ValidateTokenResponse>(
                    Config.GameApiUrl + "api/cowsino/token/validate",
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
            var roundIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var response =
                await JsonPost<PlaceBetResponse>(Config.GameApiUrl + "api/cowsino/bets/place", 
                        new PlaceBet
                        {
                            AuthToken = token,
                            Transactions = new List<PlaceBetTransaction>
                            {
                                new PlaceBetTransaction
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
        [When(@"I win \$(.*)")]
        public async Task WhenIWin(decimal amount)
        {
            var token = Get<string>(SR.token);
            var roundId = Get<string>(SR.roundId);
            var response =
                await JsonPost<PlaceBetResponse>(Config.GameApiUrl + "api/cowsino/bets/win", 
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
            var roundId = Get<string>(SR.roundId);
            var response =
                await JsonPost<LoseBetResponse>(Config.GameApiUrl + "api/cowsino/bets/lose", 
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
            var roundIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var response =
                await JsonPost<FreeBetResponse>(Config.GameApiUrl + "api/cowsino/bets/freebet", 
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
            var bets = table.CreateSet<TypeAndAmount>();

            var roundIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var i = 0;
            var response =
                await JsonPost<SettleBetsResponse>(Config.GameApiUrl + "api/cowsino/batch/bets/settle", 
                    new SettleBets
                    {
                        BatchId = NewStringId,
                        BrandKey = "138",
                        Transactions = 
                            bets.Select(taa => new BatchSettleBetTransaction
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
            var balance =
                await JsonGet<GetBalanceResponse>(Config.GameApiUrl + "api/cowsino/players/balance?authtoken=" + token);

            Set(SR.balance, balance);
        }
        [When(@"I get history")]
        public async Task WhenIGetHistory()
        {
            var token = Get<string>(SR.token);
            var history =
                await JsonGet<BetsHistoryResponse>(Config.GameApiUrl + "api/cowsino/bets/history?authtoken=" + token);

            Set(SR.response, history);
        }
        [Then(@"I will see the bet IDs in the history")]
        public void ThenIWillSeeTheroundIdsInTheHistory()
        {
            var history = Get<BetsHistoryResponse>(SR.response);
            var roundIdsList = Get<List<string>>(SR.roundIdsList);
            if(roundIdsList == null || roundIdsList.Count == 0) Assert.Fail("Expected to have placed one or more bet(s)");

            history.Should().NotBeNull();
            foreach (var roundId in roundIdsList)
            {
                if (history.Rounds.All(b => b.Id != roundId))
                {
                    Assert.Fail("Expected to see round with ID=" + roundId);
                }
            }
        }
        [When(@"I adjust transaction with \$(.*)")]
        public async Task WhenIAdjustTransactionWith(decimal amount)
        {
            var roundId = Get<string>(SR.roundId);
            var token = Get<string>(SR.token);
            var response =
                await JsonPost<AdjustTransactionResponse>(Config.GameApiUrl + "api/cowsino/transactions/adjust", 
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
            var transactionId = Get<string>(SR.transactionId);
            var response =
                await JsonPost<CancelTransactionResponse>(Config.GameApiUrl + "api/cowsino/transactions/cancel", 
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