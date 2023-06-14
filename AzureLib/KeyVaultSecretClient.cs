using Azure.Security.KeyVault.Secrets;
using System;
using System.Threading.Tasks;
using MSAzure = Azure;

namespace AzureLib;

/// <summary>
/// Class comes with functionality for dealing with Key Vault secrets, i.e. reading as well as writing secrets to Azure Key Vault. 
/// </summary>
public class KeyVaultSecretClient
{
    private string _keyVaultBaseUrl;

    private AzureTokenRetriever _azureTokenRetriever;

    /// <summary>
    /// Method retrieves secret of given <paramref name="secretName"/>. 
    /// </summary>
    /// 
    /// <param name="secretName">Name of the secret to retrieve.</param>
    /// 
    /// <returns>Latest value of the secret. If no secret of given <paramref name="secretName"/> is found, the method returns null.</returns>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="secretName"/> is null or empty.</exception>
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

    /// <summary>
    /// Method asynchronously retrieves secret of given <paramref name="secretName"/>. 
    /// </summary>
    /// 
    /// <param name="secretName">Name of the secret to retrieve.</param>
    /// 
    /// <returns>Latest value of the secret. If no secret of given <paramref name="secretName"/> is found, the method returns null.</returns>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="secretName"/> is null or empty.</exception>
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

    /// <summary>
    /// Method sets the <paramref name="secretValue"/> for Azure Key Vault secret of <paramref name="secretName"/>. 
    /// </summary>
    /// 
    /// <param name="secretName">Name of the secret.</param>
    /// <param name="secretValue">Value of the secret.</param>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="secretName"/> or <paramref name="secretValue"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="secretName"/> is empty.</exception>
    public void SetSecret(string secretName, string secretValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(secretName, nameof(secretName));
        ArgumentNullException.ThrowIfNull(secretValue);

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
        catch (Exception)
        {
            // Any required exception handling can be done here. 
            throw;
        }
    }

    /// <summary>
    /// Method asynchronously sets the <paramref name="secretValue"/> for Azure Key Vault secret of <paramref name="secretName"/>. 
    /// </summary>
    /// 
    /// <param name="secretName">Name of the secret.</param>
    /// <param name="secretValue">Value of the secret.</param>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="secretName"/> or <paramref name="secretValue"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="secretName"/> is empty.</exception>
    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(secretName, nameof(secretName));
        ArgumentNullException.ThrowIfNull(secretValue);

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
        catch (Exception)
        {
            // Any required exception handling can be done here. 
            throw;
        }
    }

    /// <summary>
    /// Constructor creates a new instance of <see cref="KeyVaultSecretClient"/> class. 
    /// </summary>
    /// 
    /// <param name="keyVaultBaseUrl">Base URL of the Azure Key Vault.</param>
    /// <param name="azureTokenRetriever">Object to be used for retrieving Azure tokens.</param>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="keyVaultBaseUrl"/> or <paramref name="azureTokenRetriever"/> 
    /// is null.</exception>
    /// 
    /// <exception cref="ArgumentException">Thrown if <paramref name="keyVaultBaseUrl"/> is null.</exception>
    public KeyVaultSecretClient(string keyVaultBaseUrl, AzureTokenRetriever azureTokenRetriever)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyVaultBaseUrl, nameof(keyVaultBaseUrl));
        ArgumentNullException.ThrowIfNull(azureTokenRetriever, nameof(azureTokenRetriever));

        this._keyVaultBaseUrl = keyVaultBaseUrl;
        this._azureTokenRetriever = azureTokenRetriever;
    }
}
