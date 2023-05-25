using Azure.Security.KeyVault.Secrets;
using System;
using System.Threading.Tasks;
using MSAzure = Azure;

namespace AzureLib;

public class KeyVaultSecretClient
{
    private string _keyVaultBaseUrl;

    private AzureTokenRetriever _azureTokenRetriever;

    public string GetSecret(string secretName)
    {
        ArgumentException.ThrowIfNullOrEmpty(secretName, nameof(secretName));

        try
        {
            MSAzure.Core.TokenCredential tokenCredential = this._azureTokenRetriever.GetClientSecretCredential();

            SecretClient secretClient = new SecretClient(new Uri(this._keyVaultBaseUrl), tokenCredential);
            MSAzure.Response<KeyVaultSecret> secret = secretClient.GetSecret(secretName);
            if ((secret != null) && (secret.Value != null))
            {
                return secret.Value.Value;
            }
        }
        catch (Exception)
        {
            // Any required exception handling can be done here. 
            throw;
        }

        return null;
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        ArgumentException.ThrowIfNullOrEmpty(secretName, nameof(secretName));

        try
        {
            MSAzure.Core.TokenCredential tokenCredential = this._azureTokenRetriever.GetClientSecretCredential();

            SecretClient secretClient = new SecretClient(new Uri(this._keyVaultBaseUrl), tokenCredential);
            MSAzure.Response<KeyVaultSecret> secret = await secretClient.GetSecretAsync(secretName);
            if ((secret != null) && (secret.Value != null))
            {
                return secret.Value.Value;
            }
        }
        catch (Exception)
        {
            // Any required exception handling can be done here. 
            throw;
        }

        return null;
    }

    public void SetSecret(string secretName, string secretValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(secretName, nameof(secretName));

        try
        {
            MSAzure.Core.TokenCredential tokenCredential = this._azureTokenRetriever.GetClientSecretCredential();

            SecretClient secretClient = new SecretClient(new Uri(this._keyVaultBaseUrl), tokenCredential);
            MSAzure.Response<KeyVaultSecret> secret = secretClient.SetSecret(secretName, secretValue);
            if ((secret != null) && (secret.Value != null))
            {
                return;
            }
            else
            {
                throw new InvalidOperationException($"Failed to set secret: {secretName}.");
            }
        }
        catch (Exception ex)
        {
            // Any required exception handling can be done here. 
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(secretName, nameof(secretName));

        try
        {
            MSAzure.Core.TokenCredential tokenCredential = this._azureTokenRetriever.GetClientSecretCredential();

            SecretClient secretClient = new SecretClient(new Uri(this._keyVaultBaseUrl), tokenCredential);
            MSAzure.Response<KeyVaultSecret> secret = await secretClient.SetSecretAsync(secretName, secretValue);
            if ((secret != null) && (secret.Value != null))
            {
                return;
            }
            else
            {
                throw new InvalidOperationException($"Failed to set secret: {secretName}.");
            }
        }
        catch (Exception ex)
        {
            // Any required exception handling can be done here. 
            throw;
        }
    }

    public KeyVaultSecretClient(string keyVaultBaseUrl, AzureTokenRetriever azureTokenRetriever)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyVaultBaseUrl, nameof(keyVaultBaseUrl));
        ArgumentNullException.ThrowIfNull(azureTokenRetriever, nameof(azureTokenRetriever));

        this._keyVaultBaseUrl = keyVaultBaseUrl;
        this._azureTokenRetriever = azureTokenRetriever;
    }
}
