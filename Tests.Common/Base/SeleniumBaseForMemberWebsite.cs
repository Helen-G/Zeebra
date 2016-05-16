using System.Web.Configuration;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class SeleniumBaseForMemberWebsite : SeleniumBase
    {
        protected override string GetWebsiteUrl()
        {
            return WebConfigurationManager.AppSettings["MemberWebsiteUrl"];
        }
    }
}