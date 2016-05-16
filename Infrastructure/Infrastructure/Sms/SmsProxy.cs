using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AFT.RegoV2.Shared;
using Common.Logging;

namespace AFT.RegoV2.Infrastructure.Sms
{
    /// <summary>
    /// Existing implementation from Ourea sources
    /// trunk\Frameworks\Aft.Infrastructure.Sms\SmsProxy.cs
    /// </summary>
    public class SmsProxy
    {
        private const string ProxyUrl = "http://bulksms.vsms.net:5567/eapi/submission/send_sms/2/2.0";
        private const string UserName = "";
        private const string Password = "";

        private static string String2Hex(string input)
        {
            var hexOutput = new StringBuilder();

            foreach (char c in input)
            {
                int tmp = c;
                hexOutput.AppendFormat("{0:X4}", Convert.ToUInt32(tmp));
            }

            return hexOutput.ToString();
        }

        public static SmsProxyResponse Send(string phonenumber, string message, string smsName = "", string routingGroup = "")
        {

            string url = ConfigurationManager.AppSettings["SmsService.Url"] ?? ProxyUrl;
            string userName = ConfigurationManager.AppSettings["SmsService.UserName"] ?? UserName;
            string password = ConfigurationManager.AppSettings["SmsService.Password"] ?? Password;

            if (phonenumber.StartsWith("8860"))
            {
                phonenumber = new Regex(@"^(8860)").Replace(phonenumber, "886");
            }

            var values = new Dictionary<string, string>();
            values.Add("username", HttpUtility.UrlEncode(userName, Encoding.GetEncoding("ISO-8859-1")));
            values.Add("password", HttpUtility.UrlEncode(password, Encoding.GetEncoding("ISO-8859-1")));
            values.Add("message", String2Hex(message));
            values.Add("sender", smsName);
            values.Add("msisdn", phonenumber);
            values.Add("dca", "16bit");
            values.Add("want_report", "1");
            if (!string.IsNullOrEmpty(routingGroup))
            {
                values.Add("routing_group", routingGroup);
            }
            string smsResult = Utils.Post(url, values);
            string[] parts = smsResult.Split('|');

            var response = new SmsProxyResponse()
            {
                Code = parts[0],
                Description = parts[1]
            };

            return response;
        }

        public class Utils
        {
            private static readonly ILog Log = LogManager.GetCurrentClassLogger();

            public static string Post(string requestUrl, Dictionary<string, string> requestArgs)
            {
                return Post2(requestUrl, ParseQuery(requestArgs));
            }

            private static string Post2(string requestUrl, string requestArgs, string sessionId = "")
            {
                var uri = new Uri(requestUrl);
                string domain = uri.Host;
                return Post3(requestUrl, requestArgs, domain, sessionId);
            }

            private static string Post3(string requestUrl, string requestArgs, string domain, string sessionId = "")
            {
                string result;

                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

                    byte[] buffer = Encoding.UTF8.GetBytes(requestArgs);

                    var request = (HttpWebRequest) WebRequest.Create(requestUrl);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = buffer.Length;

                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        request.CookieContainer = new CookieContainer();
                        var cokie = new Cookie("ASP.NET_SessionId", sessionId, "", domain);
                        request.CookieContainer.Add(cokie);
                    }

                    Stream writer = request.GetRequestStream();
                    writer.Write(buffer, 0, buffer.Length);
                    writer.Close();

                    var response = (HttpWebResponse) request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        string m1 = string.Format("post request [{0}] failed!!!", requestUrl);
                        Log.ErrorFormat(m1);
                        string m2 = string.Format("post data: [{0}]", requestArgs);
                        Log.ErrorFormat(m2);

                        throw new RegoException(string.Format("{0}, {1}", m1, m2));
                    }
                }
                catch (WebException e)
                {
                    Log.Debug(requestUrl);
                    Log.Error(e);
                    throw;
                }
                return result;
            }

            private static string ParseQuery(Dictionary<string, string> requestArgs)
            {
                var requestSb = new StringBuilder();
                if (requestArgs != null)
                {
                    foreach (var key in requestArgs.Keys)
                    {
                        requestSb.AppendFormat("&{0}={1}", key, requestArgs[key]);
                    }
                    if (requestSb.Length > 0)
                    {
                        requestSb.Remove(0, 1);
                    }
                }
                return requestSb.ToString();
            }
        }
    }
}