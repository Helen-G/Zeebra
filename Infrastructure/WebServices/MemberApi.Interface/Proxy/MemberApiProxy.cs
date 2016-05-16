using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.GameProvider;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.MemberApi.Interface.Extensions;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Security;
using Newtonsoft.Json;
using OfflineDepositRequest = AFT.RegoV2.MemberApi.Interface.Payment.OfflineDepositRequest;

namespace AFT.RegoV2.MemberApi.Interface.Proxy
{
    public class MemberApiProxy : OAuthProxy
    {
        public MemberApiProxy(string url, string token = null)
            : base(new Uri(url), token)
        {
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var result = await HttpClient.PostAsJsonAsync("api/Player/Register", request);
            return await EnsureApiResult<RegisterResponse>(result);
        }

        public async Task<ActivationResponse> ActivateAsync(string token)
        {
            var request = new ActivationRequest
            {
                Token = token
            };
            var result = await HttpClient.PostAsJsonAsync("api/Player/Activate", request);

            return await EnsureApiResult<ActivationResponse>(result);

        }

        private async Task<HttpResponseMessage> GetAccessTokenAsync(LoginRequest request)
        {
            var context = new Dictionary<string, string>
            {
                {"BrandId", request.BrandId.ToString()},
                {"IpAddress", request.IPAddress},
                {"BrowserHeaders", JsonConvert.SerializeObject(request.RequestHeaders)}
            };

            return await RequestResourceOwnerPasswordAsync("/token",
                request.Username, request.Password, additionalValues: context);
        }

        public async Task<TokenResponse> Login(LoginRequest request)
        {
            var result = await GetAccessTokenAsync(request);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                var content = await result.Content.ReadAsStringAsync();
                var details = await result.Content.ReadAsAsync<UnauthorizedDetails>();
                if (details != null && details.error_description != null)
                {
                    var apiException = JsonConvert.DeserializeObject<MemberApiException>(details.error_description);
                    throw new MemberApiProxyException(apiException, result.StatusCode);
                }

                throw new MemberApiProxyException(new MemberApiException
                {
                    ErrorMessage = content
                }, result.StatusCode);
            }

            var tokenResponse = await result.Content.ReadAsAsync<TokenResponse>();
            Token = tokenResponse.AccessToken;

            return tokenResponse;
        }

        public async Task<LogoutResponse> Logout(LogoutRequest request)
        {
            var apiResult = await HttpClient.PostAsJsonAsync("api/Player/Logout", request);

            var result = await EnsureApiResult<LogoutResponse>(apiResult);

            Token = null;

            return result;
        }

        public async Task<ProfileResponse> ProfileAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/Profile");

            return await EnsureApiResult<ProfileResponse>(result);
        }

        public async Task<SecurityQuestionsResponse> SecurityQuestionsAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/SecurityQuestions");

            return await EnsureApiResult<SecurityQuestionsResponse>(result);

        }

        public async Task<RegistrationFormDataResponse> RegistrationFormDataAsync(RegistrationFormDataRequest request)
        {
            var query = "brandId=" + request.BrandId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/RegistrationFormData", query);

            return await EnsureApiResult<RegistrationFormDataResponse>(result);
        }

        public async Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangePassword", request);

            return await EnsureApiResult<ChangePasswordResponse>(result);
        }

        public async Task<BalancesResponse> GetBalancesAsync(BalancesRequest request)
        {
            var query = "WalletId=" + request.WalletId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Player/Balances", query);

            return result.Content.ReadAsAsync<BalancesResponse>().Result;
        }

        public async Task<OfflineDepositFormDataResponse> GetOfflineDepositFormDataAsync(Guid brandId)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/OfflineDepositFormData", new OfflineDepositFormDataRequest
            {
                BrandId = brandId
            });

            return await EnsureApiResult<OfflineDepositFormDataResponse>(result);
        }

        public async Task<FundTransferFormDataResponse> GetFundTransferFormDataAsync(Guid brandId)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/FundTransferFormData", new FundTransferFormDataRequest
            {
                BrandId = brandId
            });

            return await EnsureApiResult<FundTransferFormDataResponse>(result);
        }

        public async Task<WithdrawalFormDataResponse> GetWithdrawalFormDataAsync(Guid brandId)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/WithdrawalFormData", new WithdrawalFormDataRequest
            {
                BrandId = brandId
            });

            return await EnsureApiResult<WithdrawalFormDataResponse>(result);
        }

        public async Task<OfflineDepositResponse> OfflineDepositAsync(OfflineDepositRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/OfflineDeposit", request);

            return await EnsureApiResult<OfflineDepositResponse>(result);
        }

        public async Task<OfflineWithdrawalResponse> OfflineWithdrawAsync(OfflineWithdrawalRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/OfflineWithdraw", request);

            return await EnsureApiResult<OfflineWithdrawalResponse>(result);
        }

        public async Task<FundResponse> FundInAsync(FundRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/FundIn", request);

            return await EnsureApiResult<FundResponse>(result);
        }

        public async Task<QuickDepositResponse> QuickDepositAsync(QuickDepositRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/QuickDeposit", request);

            return await EnsureApiResult<QuickDepositResponse>(result);
        }

        public async Task<ReferFriendsResponse> ReferFriendsAsync(ReferFriendsRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ReferFriends", request);

            return await EnsureApiResult<ReferFriendsResponse>(result);
        }

        public async Task<GameListResponse> GameListAsync(GameListRequest request)
        {
            var query = "playerId=" + request.PlayerId;
            var result = await HttpClient.SecureGetAsync(Token, "api/Games/GameList", query);

            return await EnsureApiResult<GameListResponse>(result);
        }

        public async Task<GameRedirectResponse> GameRedirectAsync(GameRedirectRequest request)
        {
            var query = "gameId=" + request.GameId +
                        "&gameProviderId=" + request.GameProviderId +
                        "&playerIpAddress=" + HttpUtility.UrlEncode(request.PlayerIpAddress) +
                        "&brandCode=" + HttpUtility.UrlEncode(request.BrandCode);

            var result = await HttpClient.SecureGetAsync(Token, "api/Games/GameRedirect", query);


            return await EnsureApiResult<GameRedirectResponse>(result);
        }

        public async Task<ChangePersonalInfoResponse> ChangePersonalInfoAsync(ChangePersonalInfoRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangePersonalInfo", request);

            return await EnsureApiResult<ChangePersonalInfoResponse>(result);
        }

        public async Task<ChangeContactInfoResponse> ChangeContactInfoAsync(ChangeContactInfoRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangeContactInfo", request);

            return await EnsureApiResult<ChangeContactInfoResponse>(result);
        }

        public async Task<ChangeSecurityQuestionResponse> ChangeSecurityQuestionAsync(ChangeSecurityQuestionRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/ChangeSecurityQuestion", request);

            return await EnsureApiResult<ChangeSecurityQuestionResponse>(result);
        }

        public async Task<VerificationCodeResponse> VerificationCodeAsync(VerificationCodeRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/VerificationCode", request);

            return await EnsureApiResult<VerificationCodeResponse>(result);
        }

        public async Task<VerifyMobileResponse> VerifyMobileAsync(VerifyMobileRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/VerifyMobile", request);

            return await EnsureApiResult<VerifyMobileResponse>(result);
        }

        public LanguagesResponse GetAvailableLanguages(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "api/Player/Languages", query)).Result;


            return EnsureApiResult<LanguagesResponse>(result).Result;
        }

        public async Task<BonusRedemptionsResponse> BonusRedemptionsAsync()
        {
            var result = await HttpClient.SecureGetAsync(Token, "api/Bonus/BonusRedemptions");

            return await EnsureApiResult<BonusRedemptionsResponse>(result);
        }

        public async Task<ClaimRedemptionResponse> ClaimRedemptionAsync(ClaimRedemptionRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Bonus/ClaimRedemption", request);

            return await EnsureApiResult<ClaimRedemptionResponse>(result);
        }

        public async Task<QualifyDepositBonusResponse> QualifyDepositBonusAsync(QualifyDepositBonusRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Bonus/QualifyDepositBonus", request);

            return await EnsureApiResult<QualifyDepositBonusResponse>(result);
        }

        public async Task<QualifyFundInBonusResponse> QualifyFundInBonusAsync(QualifyFundInBonusRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Bonus/QualifyFundInBonus", request);

            return await EnsureApiResult<QualifyFundInBonusResponse>(result);
        }

        // Security
        public VerifyIpResponse VerifyIp(VerifyIpRequest request)
        {
            // this call is used in an attribute. If I make this method async, the application hangs
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "api/Security/VerifyIp", request)).Result;

            return result.Content.ReadAsAsync<VerifyIpResponse>().Result;
        }

        public async Task ApplicationErrorAsync(ApplicationErrorRequest request)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Security/LogApplicationError", request);

            await EnsureApiResult<ApplicationErrorResponse>(result);
        }

        public async Task<PendingDepositsResponse> GetPendingDeposits()
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/PendingDeposits", new PendingDepositsRequest());

            return await EnsureApiResult<PendingDepositsResponse>(result);
        }

        public async Task<PlayerData> GetPlayerData(string playerName)
        {
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Player/GetPlayerData", new GetPlayerDataRequest
            {
                UserName = playerName
            });

            return await EnsureApiResult<PlayerData>(result);
        }

        public async Task<OfflineDeposit> GetOfflineDeposit(Guid id)
        {
            var request = new GetOfflineDepositRequest { Id = id };
            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/GetOfflineDeposit", request);
            return await EnsureApiResult<OfflineDeposit>(result);
        }

        public async Task<string> ConfirmOfflineDeposit(HttpRequest request)
        {
            var depositConfirm = request.Form["depositConfirm"];

            var confirm = JsonConvert.DeserializeObject<OfflineDepositConfirm>(depositConfirm);
            var uploadIdFront = request.Files["uploadId1"];
            var uploadIdBack = request.Files["uploadId2"];
            var receiptUpLoad = request.Files["receiptUpLoad"];

            confirm.IdFrontImage = uploadIdFront != null ? uploadIdFront.FileName : null;
            confirm.IdBackImage = uploadIdBack != null ? uploadIdBack.FileName : null;
            confirm.ReceiptImage = receiptUpLoad != null ? receiptUpLoad.FileName : null;

            var confirmRequest = new OfflineDepositConfirmRequest
            {
                DepositConfirm = confirm,
                IdFrontImage = uploadIdFront != null ? uploadIdFront.InputStream.ToByteArray() : null,
                IdBackImage = uploadIdBack != null ? uploadIdBack.InputStream.ToByteArray() : null,
                ReceiptImage = receiptUpLoad != null ? receiptUpLoad.InputStream.ToByteArray() : null
            };

            var result = await HttpClient.SecurePostAsJsonAsync(Token, "api/Payment/ConfirmDeposit", confirmRequest);

            return await EnsureApiResult<string>(result);
        }
    }
}
