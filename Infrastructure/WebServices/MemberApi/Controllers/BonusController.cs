using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.MemberApi.Interface.Bonus;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class BonusController : BaseApiController
    {
        private readonly BonusCommands _commands;
        private readonly IBonusQueries _bonusQueries;

        public BonusController(IBonusQueries bonusQueries, BonusCommands commands)
        {
            _bonusQueries = bonusQueries;
            _commands = commands;
        }
        
        [HttpGet]
        public BonusRedemptionsResponse BonusRedemptions()
        {
            var redemptions = _bonusQueries.GetClaimableRedemptions(PlayerId);

            return new BonusRedemptionsResponse
            {
                Redemptions = redemptions.Select(a => new ClaimableRedemption
                    {
                        Id = a.Id,
                        BonusName = a.BonusName,
                        RewardAmount = a.RewardAmount,
                        State = (int)DefineState(a.ClaimableFrom, a.ClaimableTo),
                        ClaimableFrom = a.ClaimableFrom.ToString("g"),
                        ClaimableTo = a.ClaimableTo.ToString("g")
                    }).ToArray()
            };
        }

        private ClaimableRedemptionState DefineState(DateTimeOffset durationStart, DateTimeOffset durationEnd)
        {
            var now = DateTimeOffset.Now.ToOffset(durationStart.Offset);

            return now > durationEnd ? ClaimableRedemptionState.Expired : ClaimableRedemptionState.Active;
        }

        [HttpPost]
        public ClaimRedemptionResponse ClaimRedemption(ClaimRedemptionRequest request)
        {
            _commands.ClaimBonusRedemption(PlayerId, request.RedemptionId);

            return new ClaimRedemptionResponse();
        }

        [HttpPost]
        public QualifyDepositBonusResponse QualifyDepositBonus(QualifyDepositBonusRequest request)
        {
            return new QualifyDepositBonusResponse
            {
                Errors = _bonusQueries.GetDepositQualificationFailures(PlayerId, request.BonusCode, request.Amount)
            };
        }

        [HttpPost]
        public QualifyFundInBonusResponse QualifyFundInBonus(QualifyFundInBonusRequest request)
        {
            return new QualifyFundInBonusResponse
            {
                Errors =
                    _bonusQueries.GetFundInQualificationFailures(PlayerId, request.BonusCode, request.Amount, request.WalletId)
            };
        }
    }
}