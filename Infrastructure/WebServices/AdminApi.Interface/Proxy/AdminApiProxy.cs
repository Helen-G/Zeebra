using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Extensions;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Interfaces.Admin;
using AFT.RegoV2.Core.Common.Interfaces.Brand;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminApi.Interface.Proxy
{
    public class AdminApiProxy : OAuthProxy
    {
        public AdminApiProxy(string url, string token = null) : base(new Uri(url), token)
        {
        }

        private async Task<HttpResponseMessage> GetAccessTokenAsync(LoginRequest request)
        {
            return await RequestResourceOwnerPasswordAsync("/token",
                request.Username, request.Password);
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
                    var apiException = JsonConvert.DeserializeObject<AdminApiException>(details.error_description);
                    throw new AdminApiProxyException(apiException, result.StatusCode);
                }

                throw new AdminApiProxyException(new AdminApiException
                {
                    ErrorMessage = content
                }, result.StatusCode);
            }

            var tokenResponse = await result.Content.ReadAsAsync<TokenResponse>();

            Token = tokenResponse.AccessToken;

            return tokenResponse;
        }

        #region BrandController

        public BrandsResponse GetUserBrands()
        {
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Brand/GetUserBrands")).Result;

            return EnsureApiResult<BrandsResponse>(result).Result;
        }

        //public object GetBrands(SearchPackage searchPackage)
        //{
        //    //var query = "brandId=" + brandId;
        //    //var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Brand/GetEditData", query)).Result;

        //    //return EnsureApiResult<object>(result).Result;
        //}

        public object GetBrandAddData()
        {
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Brand/GetAddData")).Result;

            return EnsureApiResult<object>(result).Result;
        }

        public object GetBrandEditData(Guid brandId)
        {
            var query = "id=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Brand/GetEditData", query)).Result;

            return EnsureApiResult<object>(result).Result;
        }

        public object GetBrandViewData(Guid brandId)
        {
            var query = "id=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Brand/GetViewData", query)).Result;

            return EnsureApiResult<object>(result).Result;
        }

        public HttpResponseMessage AddBrand(IAddBrandData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Brand/Add", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage EditBrand(IEditBrandData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Brand/Edit", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage GetBrandCountries(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Brand/GetCountries", query)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage ActivateBrand(IActivateBrandData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Brand/Activate", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage DeactivateBrand(IDeactivateBrandData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Brand/Deactivate", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public string GetBrands(bool useFilter, Guid[] licensees)
        {
            var query = "useFilter=" + useFilter;
            query += "&licensees=" + licensees;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Brand/Brands", query)).Result;

            return EnsureApiResult<string>(result).Result;
        }

        #endregion

        #region BrandCountryController

        //public object GetBrandCountriesList(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandCountry/List")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public HttpResponseMessage GetBrandCountryAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandCountry/GetAssignData", query)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage AssignBrandCountry(IAssignBrandCountryData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "BrandCountry/Assign", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        #endregion

        #region BrandCultureController

        //public object GetBrandCulturesList(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandCulture/List")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public HttpResponseMessage GetBrandCultureAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandCulture/GetAssignData", query)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage AssignBrandCulture(IAssignBrandCultureData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "BrandCulture/Assign", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage GetBrandCurrencyAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandCurrency/GetAssignData", query)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage AssignBrandCurrency(IAssignBrandCurrencyData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "BrandCurrency/Assign", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        #endregion

        #region BrandCurrencyManagerController

        //public object GetBrandCurrenciesList(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandCurrency/List")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public HttpResponseMessage GetBrandCurrencies(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandCurrency/GetBrandCurrencies", query)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        #endregion

        #region BrandProductController

        //public object GetBrandProductList(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandProduct/List")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public HttpResponseMessage GetBrandProductAssignData(Guid brandId)
        {
            var query = "brandId=" + brandId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandProduct/GetAssignData", query)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage AssignBrandProduct(IAssignBrandProductModel request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "BrandProduct/Assign", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public string GetBrandProductBetLevels(Guid brandId, Guid productId)
        {
            var query = "brandId=" + brandId;
            query += "&productId=" + productId;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "BrandProduct/BetLevels", query)).Result;

            return EnsureApiResult<string>(result).Result;
        }

        #endregion

        #region ContentTranslationController

        //public object GetContentTranslations(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "ContentTranslation/GetContentTranslations")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public HttpResponseMessage CreateContentTranslation(IAddContentTranslationModel request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "ContentTranslation/CreateContentTranslation", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        //public HttpResponseMessage UpdateContentTranslation(IEditContentTranslationData request)
        //{
        //    var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "ContentTranslation/UpdateContentTranslation", request)).Result;

        //    return EnsureApiResult<HttpResponseMessage>(result).Result;
        //}

        //public HttpResponseMessage ActivateContentTranslation(IActivateContentTranslationData request)
        //{
        //    var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "ContentTranslation/Activate", request)).Result;

        //    return EnsureApiResult<HttpResponseMessage>(result).Result;
        //}

        //public HttpResponseMessage DeactivateContentTranslation(IDeactivateContentTranslationData request)
        //{
        //    var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "ContentTranslation/Deactivate", request)).Result;

        //    return EnsureApiResult<HttpResponseMessage>(result).Result;
        //}

        //public HttpResponseMessage DeleteContentTranslation(IDeleteContentTranslationData request)
        //{
        //    var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "ContentTranslation/DeleteContentTranslation", request)).Result;

        //    return EnsureApiResult<HttpResponseMessage>(result).Result;
        //}

        //public object GetContentTranslationAddData()
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "ContentTranslation/GetContentTranslationAddData")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        //public object GetContentTranslationEditData(Guid id)
        //{
        //    var query = "id=" + id;
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "ContentTranslation/GetContentTranslationEditData", query)).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        #endregion

        #region CountryController

        //public object GetCountriesList(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Country/List")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public object GetCountryByCode(string code)
        {
            var query = "code=" + code;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Country/GetByCode", query)).Result;

            return EnsureApiResult<object>(result).Result;
        }

        public HttpResponseMessage SaveCountry(IEditCountryData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Country/Save", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage DeleteCountry(IDeleteCountryData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Country/Delete", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        #endregion

        #region CultureController

        //public object GetCulturesList(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Culture/List")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public object GetCultureByCode(string code)
        {
            var query = "code=" + code;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Culture/GetByCode", query)).Result;

            return EnsureApiResult<object>(result).Result;
        }

        public HttpResponseMessage ActivateCulture(IActivateCultureData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Culture/Activate", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }
        
        public HttpResponseMessage DeactivateCulture(IDeactivateCultureData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Culture/Deactivate", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage SaveCulture(IEditCultureData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Culture/Save", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        #endregion

        #region CurrencyController

        //public object GetCurrencyList(ISearchPackage searchPackage)
        //{
        //    var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Currency/List")).Result;

        //    return EnsureApiResult<object>(result).Result;
        //}

        public object GetCurrencyByCode(string code)
        {
            var query = "code=" + code;
            var result = Task.Run(() => HttpClient.SecureGetAsync(Token, "Currency/GetByCode", query)).Result;

            return EnsureApiResult<object>(result).Result;
        }

        public HttpResponseMessage ActivateCurrency(IActivateCurrencyData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Currency/Activate", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage DeactivateCurrency(IDeactivateCurrencyData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Currency/Deactivate", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        public HttpResponseMessage SaveCurrency(IEditCurrencyData request)
        {
            var result = Task.Run(() => HttpClient.SecurePostAsJsonAsync(Token, "Currency/Save", request)).Result;

            return EnsureApiResult<HttpResponseMessage>(result).Result;
        }

        #endregion
    }
}