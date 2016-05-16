using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AFT.RegoV2.Webservices.Adapters.SlotServer.Data
{
    public class ValidateTokenResponse
    {
        public string username { get; set; }
        public string memberCode { get; set; } 
        public string currency { get; set; }
        public string ipAddress { get; set; }
        public string statusCode { get; set; }
        public string statusDesc { get; set; }
    }
}