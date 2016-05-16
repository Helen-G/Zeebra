using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Events;
using FluentValidation;
using ServiceStack.ServiceModel.Extensions;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IEventBus _eventBus;

        public AccountController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var loginSucceeded = await LoginUser(model.Username, model.Password, model.RememberMe);

                    if (loginSucceeded)
                    {
                        _eventBus.Publish(new AdminAuthenticated(model.Username, Request));
                        string decodedUrl = "";
                        if (!string.IsNullOrEmpty(returnUrl))
                            decodedUrl = Server.UrlDecode(returnUrl);

                        if (Url.IsLocalUrl(decodedUrl))
                        {
                            return Redirect(decodedUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (ValidationException e)
                {
                    var errorMessage = e.Errors.First().ErrorMessage;
                    _eventBus.Publish(new AdminAuthenticated(model.Username, Request) { FailReason = errorMessage });
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
            }

            LogoutUser();
            return View(model);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            LogoutUser();
            return RedirectToAction("Login");
        }
    }
}