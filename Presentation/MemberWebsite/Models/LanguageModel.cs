using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberWebsite.Common;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public static class LanguageModel
    {
        private static List<Language> _availableLanguages;
        public static List<Language> AvailableLanguages
        {
            get
            {

                if (_availableLanguages != null)
                    return _availableLanguages;

                var appSettings = new AppSettings();
                var brandId = appSettings.BrandId;
                var request = new HttpRequestWrapper(HttpContext.Current.Request);
                var proxy = new MemberApiProxy(appSettings.MemberApiUrl.ToString(), request.AccessToken());

                var result = proxy.GetAvailableLanguages(brandId);

                _availableLanguages = result.Languages;
                return _availableLanguages;
                    
            }
        }

    }
}