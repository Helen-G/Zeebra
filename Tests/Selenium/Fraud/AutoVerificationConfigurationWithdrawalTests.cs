using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.Fraud
{
    [Ignore("In implementing by Igor")]
    class AutoVerificationConfigurationWithdrawalTests : SeleniumBaseForAdminWebsite
    {
        [Test]
        public void Can_not_make_withdrawal_when_player_did_not_hit_Has_Winnings_amount_criteria()
        {

        }

        [Test]
        public void Can_make_withdrawal_when_player_hits_Has_Winnings_amount_criteria()
        {

        }

    }
}

