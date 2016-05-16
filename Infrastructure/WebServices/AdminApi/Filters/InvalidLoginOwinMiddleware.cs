using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace AFT.RegoV2.AdminApi.Filters
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