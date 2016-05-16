using System;
using System.Net.Http;

namespace AFT.RegoV2.MemberApi.Interface.Proxy
{
    public class ProxyBase 
    {
        public HttpClient HttpClient { get; protected set; }

        protected ProxyBase()
        {}

        public ProxyBase(string url)
        {
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
        }

        protected ProxyBase(HttpMessageHandler handler, string url, bool disposeHandler = true)
        {
            HttpClient = new HttpClient(handler, disposeHandler)
            {
                BaseAddress = new Uri(url)
            };
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
