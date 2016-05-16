using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace AFT.RegoV2.GameWebsite.Helpers
{
    internal static class GameApiUtil
    {
        private static readonly string GameApiUrl = new AppSettings().GameApiUrl.ToString();

        internal static TResponse CallGameApiPost<TRequest, TResponse>(string path, TRequest request, Action<WebHeaderCollection> decorate)
        {
            return ProcessResponse(wc =>
                {
                    var prefix = PrepareWebClient(decorate, wc);
                    var json = JsonConvert.SerializeObject(request);

                    json = wc.UploadString(GameApiUrl + prefix + path, json);

                    return JsonConvert.DeserializeObject<TResponse>(json);
                });
        }

        internal static TResponse CallGameApiGet<TResponse>(string path, Action<WebHeaderCollection> decorate)
        {
            return ProcessResponse(wc =>
                {
                    var prefix = PrepareWebClient(decorate, wc);

                    var json = wc.DownloadString(GameApiUrl + prefix + path);

                    return JsonConvert.DeserializeObject<TResponse>(json);
                });
        }

        private static TResponse ProcessResponse<TResponse>(Func<WebClient, TResponse> process)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    return process(wc);
                }
            }
            catch (WebException webex)
            {
                if (webex.Status != WebExceptionStatus.ProtocolError) throw;
                var response = webex.Response;
                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var text = reader.ReadToEnd();
                            return JsonConvert.DeserializeObject<TResponse>(text);
                        }
                    }
                    throw new InvalidOperationException();
                }
            }
        }

        private static string PrepareWebClient(Action<WebHeaderCollection> decorate, WebClient wc)
        {
            string prefix = "";
            if (decorate != null)
            {
                prefix = "oauth/";
                decorate(wc.Headers);
            }

            wc.Headers["Content-Type"] = "application/json";
            return prefix;
        }
    }
}