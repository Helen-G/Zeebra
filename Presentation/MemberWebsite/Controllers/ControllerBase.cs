using System.Threading;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AFT.RegoV2.MemberWebsite.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected internal RedirectToRouteResult RedirectToActionLocalized(string actionName)
        {
            return RedirectToAction(actionName, new { culture = Thread.CurrentThread.CurrentUICulture.Name });
        }

        protected string SerializeJson(object response)
        {
            Response.ContentType = "application/json";
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(response, jsonSettings);
        }
    }
}
