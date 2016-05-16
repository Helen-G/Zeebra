using DotNetOpenAuth.OAuth2;

namespace AFT.RegoV2.Infrastructure.OAuth2
{
    public interface IOAuth2ClientStore
    {
        IClientDescription GetClient(string clientId);
    }
}
