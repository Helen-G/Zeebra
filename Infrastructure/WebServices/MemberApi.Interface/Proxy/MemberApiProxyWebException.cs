using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberApi.Interface.Proxy
{
    public class MemberApiProxyException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public MemberApiException Exception { get; private set; }

        public MemberApiProxyException(MemberApiException exception, HttpStatusCode code) : base(exception.ErrorMessage)
        {
            StatusCode = code;
            Exception = exception;
        }
    }
}
