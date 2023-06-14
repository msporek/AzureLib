using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;
using MSAzure = Azure;

namespace AzureLib;

public class AzureTokenRetriever
{
    public string TenantID { get; set; }

    public string ClientID { get; set; }

    public string ClientSecret { get; set; }

    public MSAzure.Identity.ClientSecretCredential GetClientSecretCredential()
    {
        return new MSAzure.Identity.ClientSecretCredential(this.TenantID, this.ClientID, this.ClientSecret);
    }

    public async Task<string> GetToken(string authority, string resource, string scope)
    {
        try
        {
            AuthenticationContext authContext = new AuthenticationContext(authority, false);
            ClientCredential credential = new ClientCredential(this.ClientID, this.ClientSecret);

            AuthenticationResult authenticationResult = await authContext.AcquireTokenAsync(resource, credential);
            return authenticationResult.AccessToken;
        }
        catch (Exception)
        {
            // TODO: Should handle the exception here. 

            return string.Empty;
        }
    }

    public AzureTokenRetriever(
        string tenantID, 
        string clientID, 
        string clientSecret)
    {
        ArgumentException.ThrowIfNullOrEmpty(tenantID, nameof(tenantID));
        ArgumentException.ThrowIfNullOrEmpty(clientID, nameof(clientID));
        ArgumentException.ThrowIfNullOrEmpty(clientSecret, nameof(clientSecret));

        this.TenantID = tenantID;
        this.ClientID = clientID;
        this.ClientSecret = clientSecret;
    }
}
