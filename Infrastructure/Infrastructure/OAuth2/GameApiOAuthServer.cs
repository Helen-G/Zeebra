using System;
using System.Collections.Generic;
using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.ChannelElements;
using DotNetOpenAuth.OAuth2.Messages;

namespace AFT.RegoV2.Infrastructure.OAuth2
{
    public class GameApiOAuthServer : IAuthorizationServerHost
    {
        private readonly ICryptoKeyStore _cryptoKeyStore;
        private readonly ICryptoKeyPair _authServerKeys;
        private readonly ICryptoKeyPair _dataServerKeys;
        private readonly IOAuth2ClientStore _clientStore;

        private const int TokenLifetimeInSeconds = 60*60; // 1 hour

        public GameApiOAuthServer(ICryptoKeyStore cryptoKeyStore,
            ICryptoKeyPair authServerKeys, ICryptoKeyPair dataServerKeys, IOAuth2ClientStore clientStore) 
        {
            _cryptoKeyStore = cryptoKeyStore;
            _authServerKeys = authServerKeys;
            _dataServerKeys = dataServerKeys;
            _clientStore = clientStore;
        }

        public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage) 
        {
            var accessToken = new AuthorizationServerAccessToken();

            // TODO: work out and implement appropriate lifespan for access tokens based on your specific application requirements
            accessToken.Lifetime = TimeSpan.FromSeconds(TokenLifetimeInSeconds);

            // TODO: artificially shorten the access token's lifetime if the original authorization is due to expire sooner than the default lifespan.
            // (i.e. don't give out 7-day access tokens to somebody who has only granted your app access for 24 hours) 

            // TODO: choose the appropriate signing keys for the specific resource server being used.
            // If you need to support multiple resource servers, there's two options.
            // 1) Use the same RSA key pair on every resource server.
            // 2) Select the appropriate key pair based on the requested scope (this assumes each scope maps to exactly one resource server)

            accessToken.ResourceServerEncryptionKey = _dataServerKeys.PublicSigningKey;
            accessToken.AccessTokenSigningKey = _authServerKeys.PrivateEncryptionKey;

            return (new AccessTokenResult(accessToken));
        }

        public IClientDescription GetClient(string clientIdentifier) 
        {
            return (_clientStore.GetClient(clientIdentifier));
        }

        public bool IsAuthorizationValid(IAuthorizationDescription authorization) 
        {
            return IsAuthorizationValid(authorization.Scope, authorization.ClientIdentifier, authorization.UtcIssued, authorization.User);
        }

        public AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest) 
        {
            return (new AutomatedUserAuthorizationCheckResponse(accessRequest, true, userName));
            //TODO: this is where the OAuth2 server checks the client username/password are correct. Replace this with your own implementation.
            /*var user = _userStore.GetUser(userName);
            var approved = (user != null && user.VerifyPassword(password));
            return (new AutomatedUserAuthorizationCheckResponse(accessRequest, approved, userName));*/
        }

        public AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest) 
        {
            //TODO: if you're using web-based redirect login, this is where you verify the client's access token request is valid.
            return (new AutomatedAuthorizationCheckResponse(accessRequest, true));
        }

        public ICryptoKeyStore CryptoKeyStore { get { return (_cryptoKeyStore); } }
        public INonceStore NonceStore { get { throw (new NotImplementedException()); } }


        public bool CanBeAutoApproved(EndUserAuthorizationRequest authorizationRequest) 
        {
            //TODO: make sure we have a valid client secret on file matching that included with the authorization request
            return (true);
            //if (authorizationRequest == null) {
            //    throw new ArgumentNullException("authorizationRequest");
            //}

            //// NEVER issue an auto-approval to a client that would end up getting an access token immediately
            //// (without a client secret), as that would allow arbitrary clients to masquarade as an approved client
            //// and obtain unauthorized access to user data.
            //if (authorizationRequest.ResponseType == EndUserAuthorizationResponseType.AuthorizationCode) {
            //    // Never issue auto-approval if the client secret is blank, since that too makes it easy to spoof
            //    // a client's identity and obtain unauthorized access.
            //    var requestingClient = MvcApplication.DataContext.Clients.First(c => c.ClientIdentifier == authorizationRequest.ClientIdentifier);
            //    if (!string.IsNullOrEmpty(requestingClient.ClientSecret)) {
            //        return this.IsAuthorizationValid(
            //            authorizationRequest.Scope,
            //            authorizationRequest.ClientIdentifier,
            //            DateTime.UtcNow,
            //            HttpContext.Current.User.Identity.Name);
            //    }
            //}

            //// Default to not auto-approving.
            //return false;
        }

        private bool IsAuthorizationValid(HashSet<string> requestedScopes, string clientIdentifier, DateTime issuedUtc, string username) 
        {
            // TODO: This is where we should check (e.g. in our authorization database) that the resource owner has not revoked the authorization associated with the request.
            return (true);
            // If db precision exceeds token time precision (which is common), the following query would
            // often disregard a token that is minted immediately after the authorization record is stored in the db.
            // To compensate for this, we'll increase the timestamp on the token's issue date by 1 second.
            //issuedUtc += TimeSpan.FromSeconds(1);
            //var grantedScopeStrings = from auth in MvcApplication.DataContext.ClientAuthorizations
            //                          where
            //                              auth.Client.ClientIdentifier == clientIdentifier &&
            //                              auth.CreatedOnUtc <= issuedUtc &&
            //                              (!auth.ExpirationDateUtc.HasValue || auth.ExpirationDateUtc.Value >= DateTime.UtcNow) &&
            //                              auth.User.OpenIDClaimedIdentifier == username
            //                          select auth.Scope;

            //if (!grantedScopeStrings.Any()) {
            //    // No granted authorizations prior to the issuance of this token, so it must have been revoked.
            //    // Even if later authorizations restore this client's ability to call in, we can't allow
            //    // access tokens issued before the re-authorization because the revoked authorization should
            //    // effectively and permanently revoke all access and refresh tokens.
            //    return false;
            //}

            //var grantedScopes = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
            //foreach (string scope in grantedScopeStrings) {
            //    grantedScopes.UnionWith(OAuthUtilities.SplitScopes(scope));
            //}

            //return requestedScopes.IsSubsetOf(grantedScopes);
        }
    }
}