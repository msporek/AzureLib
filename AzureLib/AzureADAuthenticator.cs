using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace AzureLib;

public class AzureADAuthenticator
{
    public AzureADAuthenticator()
    {
    }

    public async Task<AuthenticationResult> SignIn(string clientID)
    {
        ArgumentNullException.ThrowIfNull(clientID, nameof(clientID));

        new InternetOptionsHandler().ClearCookies();

        return await this.AcquireAccessToken(clientID, PromptBehavior.Always);
    }

    public async Task<AuthenticationResult> AcquireAccessToken(string clientID, PromptBehavior promptBehavior)
    {
        ArgumentNullException.ThrowIfNull(clientID, nameof(clientID));

        string resourceUri = "https://graph.windows.net";
        string redirectUri = "http://localhost";
        string authorityUri = "https://login.windows.net/common/oauth2/authorize";

        return await this.AcquireAccessToken(clientID, promptBehavior, redirectUri, resourceUri, authorityUri);
    }

    public async Task<AuthenticationResult> AcquireAccessToken(string clientID, PromptBehavior promptBehavior, string redirectURI, string resourceURI, string authorityURI)
    {
        ArgumentNullException.ThrowIfNull(clientID, nameof(clientID));
        ArgumentNullException.ThrowIfNull(redirectURI, nameof(redirectURI));
        ArgumentNullException.ThrowIfNull(resourceURI, nameof(resourceURI));
        ArgumentNullException.ThrowIfNull(authorityURI, nameof(authorityURI));

        AuthenticationContext authContext = new AuthenticationContext(authorityURI, false, null);
        if (authContext != null && authContext.TokenCache != null)
        {
            authContext.TokenCache.Clear();
        }

        IPlatformParameters platformParameters = new PlatformParameters(promptBehavior, null);
        return await authContext.AcquireTokenAsync(resourceURI, clientID, new Uri(redirectURI), platformParameters);
    }
}
