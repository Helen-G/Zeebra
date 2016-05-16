using System.Web.Http;

namespace AFT.RegoV2.AdminApi.Controllers
{
    public class RootController : ApiController
    {
        [Route("")]
        [AllowAnonymous]
        public string[] Get()
        {
            return new[] {"RegoV2 Admin API"};
        }
    }
}