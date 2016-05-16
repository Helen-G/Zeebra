using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AFT.RegoV2.AdminApi.Interface.Proxy;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public static class Extensions
    {
        //public static string ActionLocalized(this UrlHelper url, string actionName, string controllerName = null)
        //{
        //    return controllerName != null
        //        ? url.Action(actionName, controllerName, new { culture = Thread.CurrentThread.CurrentUICulture.Name })
        //        : url.Action(actionName, new { culture = Thread.CurrentThread.CurrentUICulture.Name });
        //}

        public static string AccessToken(this HttpRequestBase request)
        {
            var cookie = request.Cookies[FormsAuthentication.FormsCookieName];
            string token = null;
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
            {
                var formsTicket = FormsAuthentication.Decrypt(cookie.Value);
                if (formsTicket != null)
                {
                    token = formsTicket.UserData;
                }
            }
            return token;
        }

        public static Dictionary<string, string> ToDictionary(this NameValueCollection nameValues)
        {
            if (nameValues == null) return new Dictionary<string, string>();

            var map = new Dictionary<string, string>();
            foreach (var key in nameValues.AllKeys)
            {
                if (key == null)
                {
                    continue;
                }

                var values = nameValues.GetValues(key);
                if (values != null && values.Length > 0)
                {
                    map[key] = values[0];
                }
            }
            return map;
        }

       
    }
}