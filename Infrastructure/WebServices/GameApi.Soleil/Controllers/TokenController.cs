using System.Net.Http;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.GameApi.Interface.Extensions;
using AFT.RegoV2.GameApi.Interface.ServiceContracts.OAuth;
using AFT.RegoV2.Infrastructure.Attributes;
using AFT.RegoV2.Infrastructure.OAuth2;
using DotNetOpenAuth.OAuth2;

namespace AFT.RegoV2.GameApi.ACS.Soleil.Controllers
{
    [ForceJsonFormatter("multipart/form-data", "application/x-www-form-urlencoded")]//additionalMediaTypes
    public class SoleilTokenController : ApiController
    {
        private readonly AuthorizationServer _authServer;

        public SoleilTokenController(IGameRepository repository)
        {
            var authCertificateLocation = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["CertificateLocation"]);

            var authCryptoKeyPair = CryptoKeyPair.LoadCertificate(authCertificateLocation, WebConfigurationManager.AppSettings["CertificatePassword"]);

            var gameProviderStore = new GameProviderOAuthStore(repository);

            var regoAuthServer = new GameApiOAuthServer(new CryptoKeyStore(),
                authCryptoKeyPair,
                authCryptoKeyPair,
                gameProviderStore);
            _authServer = new AuthorizationServer(regoAuthServer);
        }
        [Route("api/soleil/oauth/token")]
        public HttpResponseMessage Post([FromBody]OAuth2Token request)
        {
            var result = _authServer.HandleTokenRequest(Request.GetRequestBase());

            return new HttpResponseMessage
            {
                Content = new StringContent(result.Body, Encoding.UTF8, "application/json")
            };
        }
    }
}
