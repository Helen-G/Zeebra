using System.Collections.Generic;
using System.Web;
using AFT.RegoV2.Core.Common.Interfaces;
using ServiceStack.ServiceModel.Extensions;

namespace AFT.RegoV2.Core.Security.Events
{
    public class AdminAuthenticated : DomainEventBase
    {
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string FailReason { get; set; }

        public AdminAuthenticated() { }
        public AdminAuthenticated(string username, HttpRequestBase request)
        {
            Username = username;
            if (request != null)
            {
                IPAddress = request.ServerVariables["REMOTE_ADDR"];
                Headers = request.Headers.ToDictionary();
            }
        }
    }
}
