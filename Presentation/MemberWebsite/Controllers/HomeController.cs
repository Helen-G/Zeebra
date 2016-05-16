using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.MemberApi.Interface.GameProvider;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Interface.Security.IpFiltering;
using AFT.RegoV2.MemberWebsite.Models;
using AFT.RegoV2.MemberWebsite.Resources;
using AFT.RegoV2.MemberWebsite.Common;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberWebsite.Controllers
{
    public class HomeController : ControllerBase
    {
        public const string BrandCode = "138";
        public const string BrandName = "138";


        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Index()
        {
            return RedirectPermanent("Login");
        }

        [AuthorizeIpAddress(BrandCode)]
        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToActionLocalized("PlayerProfile");
            }

            return View();
        }

        [Authorize]
        public async Task<ActionResult> OfflineDepositConfirm(Guid id)
        {
            var memberApi = GetMemberApiProxy(Request);
            var deposit = await memberApi.GetOfflineDeposit(id);

            return View(new OfflineDepositConfirmationModel
            {
                Deposit = deposit
            });
        }
        
        [Authorize]
        public async Task<ActionResult> OfflineDepositResubmit(Guid id)
        {
            var memberApi = GetMemberApiProxy(Request);
            var deposit = await memberApi.GetOfflineDeposit(id);

            return View("OfflineDepositConfirm", new OfflineDepositConfirmationModel
            {
                Deposit = deposit
            });
        }

        [AuthorizeIpAddress(BrandCode), HttpPost]
        public async Task<JsonResult> Login(LoginRequest model)
        {
            const string IPAddressServerVariableName = "REMOTE_ADDR";

            var appSettings = new AppSettings();
            var brandId = appSettings.BrandId;

            model.BrandId = brandId;
            model.IPAddress = Request.ServerVariables[IPAddressServerVariableName];
            model.RequestHeaders = Request.Headers.ToDictionary();

            var loginResult = await GetMemberApiProxy(Request).Login(model);
            return Json(loginResult, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult PlayerProfile()
        {
            return View();
        }

        public async Task<ActionResult> Register()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToActionLocalized("PlayerProfile");
            }

            var settings = new AppSettings();
            var accessProxy = GetMemberApiProxy(Request);
            var result = await accessProxy.RegistrationFormDataAsync(new RegistrationFormDataRequest
            {
                BrandId = settings.BrandId
            });

            return View(result);
        }

        [HttpPost]
        public async Task<JsonResult> Register(RegisterRequest model)
        {
            var registerResult = await GetMemberApiProxy(Request).RegisterAsync(model);
            return Json(registerResult, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public async Task<ActionResult> RegisterStep2()
        {

            var settings = new AppSettings();
            var brandId = settings.BrandId;

            var memberApi = GetMemberApiProxy(Request);
            var offlineDeposit = await memberApi.GetOfflineDepositFormDataAsync(brandId);
            
            ViewData.Add("BrandName", BrandName);

            return View(offlineDeposit);
        }

        [Authorize]
        public ActionResult RegisterStep3(decimal amount)
        {
            var model = new RegisterStep3Model
            {
                DepositAmount = amount,
                BonusAmount = 150,
                BrandName = BrandName
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Activate(string token)
        {
            var result = await GetMemberApiProxy(Request)
                .ActivateAsync(token);
            ViewBag.Activated = result.Activated;
            return View(result);

        }

        [Authorize]
        public async Task<ActionResult> GameList()
        {
            var result = await GetMemberApiProxy(Request).GameListAsync(new GameListRequest());
            return View(result);
        }

        [Authorize]
        public async Task<ActionResult> RedirectToGame(Guid gameId, Guid gameProviderId)
        {
            var result = await GetMemberApiProxy(Request).GameRedirectAsync(new GameRedirectRequest
            {
                GameId = gameId,
                GameProviderId = gameProviderId,
                PlayerIpAddress = Request.UserHostAddress,
                BrandCode = BrandCode
            });

            if (result.IsPostRequest)
            {
                return Content(GeneratePostRequest(result.Url.OriginalString));
            }

            return Redirect(result.Url.ToString());
        }

        private static readonly char[] QuestrionMarkSplitter = { '?' };
        private string GeneratePostRequest(string url)
        {
            var arr = url.Split(QuestrionMarkSplitter, 2);
            url = arr[0];
            var vals = arr.Length > 1 ? HttpUtility.ParseQueryString(arr[1]) : new NameValueCollection();
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
            sb.AppendLine("<form id=\"theForm\" action=\"" + url + "\" method=\"POST\">");
            foreach (string key in vals.Keys)
            {
                sb.AppendLine("<input type=\"hidden\" name=\"" + key + "\" value=\"" + vals[key] + "\" />");
            }
            sb.AppendLine("</form>");
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("document.getElementById(\"theForm\").submit();");
            sb.AppendLine("</script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        public ActionResult SetCulture(string cultureCode, string returnPath = "/")
        {
            var cookie = new HttpCookie("CultureCode", cultureCode) { Expires = DateTime.Now.AddYears(1) };
            Response.SetCookie(cookie);
            return Redirect(returnPath);
        }

        [Authorize]
        public ActionResult ReferAFriend()
        {
            return View();
        }

        [Authorize]
        public ActionResult ClaimBonusReward()
        {
            return View();
        }

        [Authorize]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> BalanceInformation()
        {
            var memberApi = GetMemberApiProxy(Request);
            var balances = await memberApi.GetBalancesAsync(new BalancesRequest());

            var settings = new AppSettings();
            var brandId = settings.BrandId;

            var offlineDeposit = await memberApi.GetOfflineDepositFormDataAsync(brandId);
            var fundTransfer = await memberApi.GetFundTransferFormDataAsync(brandId);
            var withdrawal = await memberApi.GetWithdrawalFormDataAsync(brandId);
            var pedingDeposits = await memberApi.GetPendingDeposits();

            var model = new BalanceInformationModel
            {
                Balances = balances,
                OfflineDeposit = offlineDeposit,
                FundTransfer = fundTransfer,
                Withdrawal = withdrawal,
                PendingDeposits = pedingDeposits
            };
            return View(model);
        }

        [Authorize]
        public async Task<string> GetProductBalance(Guid? walletId)
        {
            var balances = await GetMemberApiProxy(Request).GetBalancesAsync(new BalancesRequest { WalletId = walletId });

            return SerializeJson(new
            {
                balances.Main,
                balances.Bonus,
                balances.Free,
                balances.Playable
            });
        }

        public ActionResult Menu(string currentController, string currentAction)
        {
            var model = new Menu(currentController, currentAction)
            {
                Items = Request.IsAuthenticated
                    ? new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Text = Labels.Menu_Home,
                            Action = "PlayerProfile",
                            SubMenuItems = new List<MenuItem>
                            {
                                new MenuItem { Text = Labels.Menu_PlayerProfile_PlayGames, Action = "GameList" },
                                new MenuItem { Text = Labels.Menu_PlayerProfile_Personal, Action = "PlayerProfile" },
                                new MenuItem { Text = Labels.Menu_PlayerProfile_ReferFriend, Action = "ReferAFriend" },
                                new MenuItem { Text = Labels.Menu_PlayerProfile_ClaimBonus, Action = "ClaimBonusReward" },
                                new MenuItem { Text = Labels.Menu_PlayerProfile_BalanceInformation, Action = "BalanceInformation" }
                            }
                        }
                    }
                    : new List<MenuItem>
                    {
                        new MenuItem { Text = Labels.Menu_Home, Action = "Login" },
                        new MenuItem { Text = Labels.Menu_Register, Action = "Register" },
                    }
            };
            model.Items.AddRange(new[]
            {
                new MenuItem { Text = Labels.Menu_Casino},
                new MenuItem { Text = Labels.Menu_LiveCasino },
                new MenuItem { Text = Labels.Menu_Bingo },
                new MenuItem { Text = Labels.Menu_Cashier },
                new MenuItem { Text = Labels.Menu_Promotions },
                new MenuItem { Text = Labels.Menu_News },
                new MenuItem { Text = Labels.Menu_Support }
            });
            return PartialView("_PartialMenu", model);
        }

        private MemberApiProxy GetMemberApiProxy(HttpRequestBase request)
        {
            var appSettings = new AppSettings();
            return new MemberApiProxy(appSettings.MemberApiUrl.ToString(), request.AccessToken());
        }

    }
}
