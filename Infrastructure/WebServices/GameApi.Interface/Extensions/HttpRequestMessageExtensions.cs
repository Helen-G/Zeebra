using System.Net.Http;
using System.Web;

namespace AFT.RegoV2.GameApi.Interface.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestBase GetRequestBase(this HttpRequestMessage request)
        {
            return ((HttpContextWrapper) request.Properties["MS_HttpContext"]).Request;
        }
        public static HttpContext GetHttpContext(this HttpRequestMessage request)
        {
            return ((HttpContextWrapper) request.Properties["MS_HttpContext"]).ApplicationInstance.Context;
        }
    }
}