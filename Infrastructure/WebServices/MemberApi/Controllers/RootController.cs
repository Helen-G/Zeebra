
using System.Web.Http;


namespace AFT.RegoV2.MemberApi.Controllers
{
    public class RootController : ApiController
    {
        [Route("")]
        [AllowAnonymous]
        public string[] Get()
        {
            return new[] {"REGO 2 Member API"};
        }
    }

}