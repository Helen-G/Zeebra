using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.MemberApi.Interface
{
    public static class HttpClientExtensions
    {
        public static void SetToken(this HttpClient client, string scheme, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
        }

        public static void SetBearerToken(this HttpClient client, string token)
        {
            client.SetToken("Bearer", token);
        }

        public static async Task<HttpResponseMessage> SecurePostAsJsonAsync<T>(this HttpClient client, string token, string url, T request)
        {
            if (token != null)
            {
                client.SetBearerToken(token);
            }
            return await client.PostAsJsonAsync(url, request);
        }

        public static async Task<HttpResponseMessage> SecureGetAsync(this HttpClient client, string token, string url, string query = "")
        {
            if (token != null)
            {
                client.SetBearerToken(token);
            }
            return await client.GetAsync(url + "?" + query);
        }

    }
}
