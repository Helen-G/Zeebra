using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AFT.RegoV2.MemberApi.Interface;
using Microsoft.Owin;

namespace AFT.RegoV2.MemberApi.Filters
{
    public class InvalidLoginOwinMiddleware : OwinMiddleware
    {
        public const string InvalidLoginHeader = "X-Invalid-Login";

        public InvalidLoginOwinMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);

            if (context.Response.Headers.ContainsKey(InvalidLoginHeader))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.Headers.Remove(InvalidLoginHeader);
            }
        }
    }
}