using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using ServiceStack.ServiceModel.Extensions;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Core.Security.Events
{
    public class MemberAuthenticationSucceded : DomainEventBase
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

    public class MemberAuthenticationFailed : DomainEventBase
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string FailReason { get; set; }
    }

}
