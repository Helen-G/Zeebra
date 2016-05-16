using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.AdminApi.Interface.Common;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminApi.Interface.Proxy
{
    public class OAuthProxy : ProxyBase
    {
        protected ClientAuthenticationStyle _authenticationStyle;
        protected string Token;

        public enum ClientAuthenticationStyle
        {
            PostValues,
            None
        };

        protected Uri _address;
        protected string _clientId;
        protected string _clientSecret;

        protected OAuthProxy(Uri address, string token)
            : this(address, new HttpClientHandler())
        {
            Token = token;
        }

        protected OAuthProxy(Uri address, HttpMessageHandler innerHttpClientHandler)
        {
            if (innerHttpClientHandler == null)
            {
                throw new ArgumentNullException("innerHttpClientHandler");
            }

            HttpClient = new HttpClient(innerHttpClientHandler)
            {
                BaseAddress = address
            };

            _address = address;
            _authenticationStyle = ClientAuthenticationStyle.None;
        }

        protected OAuthProxy(Uri address, string clientId, string clientSecret, ClientAuthenticationStyle style = ClientAuthenticationStyle.PostValues)
            : this(address, clientId, clientSecret, new HttpClientHandler(), style)
        { }

        protected OAuthProxy(Uri address, string clientId, string clientSecret, HttpMessageHandler innerHttpClientHandler, ClientAuthenticationStyle style = ClientAuthenticationStyle.PostValues)
            : this(address, innerHttpClientHandler)
        {
            if (style == ClientAuthenticationStyle.PostValues)
            {
                _authenticationStyle = style;
                _clientId = clientId;
                _clientSecret = clientSecret;
            }
        }

        public Task<HttpResponseMessage> RequestResourceOwnerPasswordAsync(string tokenPath, string userName, string password, string scope = null, Dictionary<string, string> additionalValues = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fields = new Dictionary<string, string>
            {
                { OAuth2Constants.GrantType, OAuth2Constants.GrantTypes.Password },
                { OAuth2Constants.UserName, userName },
                { OAuth2Constants.Password, password }
            };

            if (!string.IsNullOrWhiteSpace(scope))
            {
                fields.Add(OAuth2Constants.Scope, scope);
            }

            return RequestAsync(tokenPath, Merge(fields, additionalValues), cancellationToken);
        }

        public async Task<HttpResponseMessage> RequestAsync(string address, Dictionary<string, string> form, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await HttpClient.PostAsync(address, new FormUrlEncodedContent(form), cancellationToken).ConfigureAwait(false);
        }

        private Dictionary<string, string> Merge(Dictionary<string, string> explicitValues, Dictionary<string, string> additionalValues = null)
        {
            var merged = explicitValues;

            if (_authenticationStyle == ClientAuthenticationStyle.PostValues)
            {
                merged.Add(OAuth2Constants.ClientId, _clientId);
                merged.Add(OAuth2Constants.ClientSecret, _clientSecret);
            }

            if (additionalValues != null)
            {
                merged =
                    explicitValues.Concat(additionalValues.Where(add => !explicitValues.ContainsKey(add.Key)))
                                         .ToDictionary(final => final.Key, final => final.Value);
            }

            return merged;
        }

        protected async Task<T> EnsureApiResult<T>(HttpResponseMessage result)
        {
            if (result.StatusCode != HttpStatusCode.OK)
            {

                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    var apiException =
                        new AdminApiException
                        {
                            ErrorMessage = "Method not found"
                        };
                    throw new AdminApiProxyException(apiException, result.StatusCode);
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.BadRequest)
                {
                    var details = result.Content.ReadAsAsync<UnauthorizedDetails>().Result;
                    var apiException = JsonConvert.DeserializeObject<AdminApiException>(details.error_description);
                    throw new AdminApiProxyException(apiException, result.StatusCode);
                }
                else
                {
                    var apiException = result.Content.ReadAsAsync<AdminApiException>().Result;
                    throw new AdminApiProxyException(apiException, result.StatusCode);
                }
            }

            var response = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(response);
            //return await result.Content.ReadAsAsync<T>();
        }
    }

    internal class UnauthorizedDetails
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
