using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberWebsite.Common;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberWebsite.Controllers
{
    public class MemberApiController : ApiController
    {
        private readonly Guid _brandId;
        private readonly MemberApiProxy _accountProxy;

        public MemberApiController()
        {
            var appSettings = new AppSettings();
            _brandId = appSettings.BrandId;

            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            string token = null;
            if (cookie != null && !String.IsNullOrWhiteSpace(cookie.Value))
            {
                try
                {
                    var formsTicket = FormsAuthentication.Decrypt(cookie.Value);
                    if (formsTicket != null)
                    {
                        token = formsTicket.UserData;
                    }
                }
                catch (CryptographicException)
                {

                }
            }
            _accountProxy = new MemberApiProxy(appSettings.MemberApiUrl.ToString(), token);
        }

        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            const string IPAddressServerVariableName = "REMOTE_ADDR";

            request.BrandId = _brandId;

            var httpRequest = ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request;

            request.IPAddress = httpRequest.ServerVariables[IPAddressServerVariableName];
            request.RequestHeaders = httpRequest.Headers.ToDictionary();

            var result = await _accountProxy.Login(request);

            var cookie = CreateAuthenticationCookie(result.AccessToken, request.Username, false);
            HttpContext.Current.Response.Cookies.Add(cookie);
            return Ok(new { Success = true });
        }

        public IHttpActionResult Logout(LogoutRequest request)
        {
            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                cookie.Expires = DateTime.Now.AddDays(-10);
                cookie.Value = null;
                HttpContext.Current.Response.SetCookie(cookie);
            }
            return Ok(new { });
        }

        public async Task<IHttpActionResult> Register(RegisterRequest request)
        {
            await _accountProxy.RegisterAsync(request);
            return await Login(new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            });
        }

        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> Profile()
        {
            var profile = await _accountProxy.ProfileAsync();
            return Ok(new { Success = true, Profile = profile });
        }

        [System.Web.Http.HttpGet]
        public async Task<SecurityQuestionsResponse> SecurityQuestions()
        {
            return await _accountProxy.SecurityQuestionsAsync();
        }

        public async Task<ChangePasswordResponse> ChangePassword(ChangePasswordRequest request)
        {
            return await _accountProxy.ChangePasswordAsync(request);
        }

        public async Task<QuickDepositResponse> QuickDeposit(QuickDepositRequest request)
        {
            return await _accountProxy.QuickDepositAsync(request);
        }

        public async Task<OfflineDepositResponse> OfflineDeposit(OfflineDepositRequest request)
        {
            return await _accountProxy.OfflineDepositAsync(request);
        }

        public async Task<OfflineWithdrawalResponse> OfflineWithdrawal(OfflineWithdrawalRequest request)
        {
            return await _accountProxy.OfflineWithdrawAsync(request);
        }


        public async Task<FundResponse> FundIn(FundRequest request)
        {
            return await _accountProxy.FundInAsync(request);
        }

        public async Task<BonusRedemptionsResponse> GetBonusRedemptions()
        {
            return await _accountProxy.BonusRedemptionsAsync();
        }

        public async Task<ClaimRedemptionResponse> ClaimBonusReward(ClaimRedemptionRequest request)
        {
            return await _accountProxy.ClaimRedemptionAsync(request);
        }

        public async Task<ReferFriendsResponse> ReferFriends(ReferFriendsRequest request)
        {
            return await _accountProxy.ReferFriendsAsync(request);
        }

        public async Task<QualifyDepositBonusResponse> QualifyDepositBonus(QualifyDepositBonusRequest request)
        {
            return await _accountProxy.QualifyDepositBonusAsync(request);
        }

        public async Task<QualifyFundInBonusResponse> QualifyFundInBonus(QualifyFundInBonusRequest request)
        {
            return await _accountProxy.QualifyFundInBonusAsync(request);
        }


        public async Task<ChangePersonalInfoResponse> ChangePersonalInfo(ChangePersonalInfoRequest request)
        {
            return await _accountProxy.ChangePersonalInfoAsync(request);
        }

        [System.Web.Http.HttpPost]
        public async Task<dynamic> ConfirmOfflineDeposit()
        {
            var request = HttpContext.Current.Request;
            
            try
            {
                 await _accountProxy.ConfirmOfflineDeposit(request);
            }
            catch (Exception exception)
            {
                return new
                {
                    Result = "failed",
                    Message = exception.Message
                };
            }

            return new
            {
                Result = "success"
            };
        }

        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetOfflineDeposit(Guid id)
        {
            var deposit = await _accountProxy.GetOfflineDeposit(id);
            var playerName = HttpContext.Current.User.Identity.Name;
            var playerData = await _accountProxy.GetPlayerData(playerName);

            return Ok(new
            {
                Deposit = deposit,
                Player = playerData
            });
        }

        public async Task<ChangeSecurityQuestionResponse> ChangeSecurityQuestion(ChangeSecurityQuestionRequest request)
        {
            return await _accountProxy.ChangeSecurityQuestionAsync(request);
        }

        public async Task<VerificationCodeResponse> VerificationCode(VerificationCodeRequest request)
        {
            return await _accountProxy.VerificationCodeAsync(request);
        }

        public async Task<VerifyMobileResponse> VerifyMobile(VerifyMobileRequest request)
        {
            return await _accountProxy.VerifyMobileAsync(request);
        }




        private static HttpCookie CreateAuthenticationCookie(string oauthToken, string userName, bool rememberMe)
        {
            var ticket = new FormsAuthenticationTicket(
                1,
                userName,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                rememberMe,
                oauthToken);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            return new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
        }

    }
}