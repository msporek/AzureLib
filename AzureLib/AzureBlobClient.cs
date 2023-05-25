using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using AzureLib.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSAzure = Azure;

namespace AzureLib
{
    public class AzureBlobClient
    {
        public static readonly string ConnectionStringPattern =
            $"DefaultEndpointsProtocol=https;" +
            "AccountName={AccountName};" +
            "AccountKey={AccountKey};" +
            "EndpointSuffix=core.windows.net";

        private string _storageAccountConnectionString;

        public AzureBlobClient(string storageAccountName, string storageAccountKey)
        {
            ArgumentException.ThrowIfNullOrEmpty(storageAccountName, nameof(storageAccountName));
            ArgumentException.ThrowIfNullOrEmpty(storageAccountKey, nameof(storageAccountKey));

            this._storageAccountConnectionString = 
                ConnectionStringPattern
                    .Replace("{AccountName}", storageAccountName)
                    .Replace("{AccountKey}", storageAccountKey);
        }

        public AzureBlobClient(string storageAccountConnectionString)
        {
            ArgumentException.ThrowIfNullOrEmpty(storageAccountConnectionString, nameof(storageAccountConnectionString));

            this._storageAccountConnectionString = storageAccountConnectionString;
        }

        public void CreateContainer(string blobContainerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));

            BlobContainerClient blobContainerClient = new BlobContainerClient(this._storageAccountConnectionString, blobContainerName);
            MSAzure.Response<bool> blobContainerExists = blobContainerClient.Exists();
            if (!blobContainerExists.Value)
            {
                blobContainerClient.Create();
            }
        }

        public void CreateContainerRemoveContentsIfExists(string blobContainerName, int maxReattempts = 10)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));

            BlobContainerClient blobContainerClient = new BlobContainerClient(this._storageAccountConnectionString, blobContainerName);
            MSAzure.Response<bool> blobContainerExists = blobContainerClient.Exists();
            if (!blobContainerExists.Value)
            {
                blobContainerClient.Create();
            }
            else
            {
                int reattempts = 0;
                try
                {
                    List<BlobItem> allBlobItems = blobContainerClient.GetAllBlobs();
                    foreach (BlobItem blobItem in allBlobItems)
                    {
                        blobContainerClient.DeleteBlob(blobItem.Name);
                    }
                }
                catch (Exception)
                {
                    if (reattempts > maxReattempts)
                    {
                        throw;
                    }
                    else
                    {
                        reattempts++;
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(15));
                    }
                }
            }
        }

        public bool CheckBlobExists(string blobContainerName, string blobName)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

            BlobContainerClient blobContainerClient = new BlobContainerClient(this._storageAccountConnectionString, blobContainerName);
            MSAzure.Response<bool> blobContainerExists = blobContainerClient.Exists();
            if (!blobContainerExists.Value)
            {
                return false;
            }

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            MSAzure.Response<bool> blobExists = blobClient.Exists();
            if (!blobExists.Value)
            {
                return false;
            }

            return true;
        }

        public byte[] DownloadBlob(string blobContainerName, string blobName, bool throwExceptionIfNotExists = true)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

            BlobContainerClient blobContainerClient = new BlobContainerClient(this._storageAccountConnectionString, blobContainerName);
            MSAzure.Response<bool> blobContainerExists = blobContainerClient.Exists();
            if (!blobContainerExists.Value)
            {
                if (throwExceptionIfNotExists)
                {
                    throw new Exception($"No container was found of name: {blobContainerName}.");
                }
                else
                {
                    return null;
                }
            }

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            MSAzure.Response<bool> blobExists = blobClient.Exists();
            if (!blobExists.Value)
            {
                if (throwExceptionIfNotExists)
                {
                    throw new Exception($"Blob {blobName} was not found in container {blobContainerName}.");
                }
                else
                {
                    return null;
                }
            }

            return blobContainerClient.DownloadBlobBytes(blobName);
        }

        public string UploadBlob(
            string blobContainerName,
            string blobName,
            byte[] blobData,
            TimeSpan sasExpireTimeSpan,
            bool deleteIfExists = true,
            bool returnSaS = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
            ArgumentNullException.ThrowIfNull(blobData, nameof(blobData));

            if (!blobData.Any())
            {
                throw new ArgumentNullException("Value of the argument was not provided.", nameof(blobData));
            }

            BlobContainerClient blobContainerClient = new BlobContainerClient(this._storageAccountConnectionString, blobContainerName);
            blobContainerClient.CreateIfNotExists();

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            if (deleteIfExists)
            {
                if (blobClient.Exists())
                {
                    blobClient.DeleteIfExists();
                }
            }

            this.UploadBlob(blobContainerClient, blobName, blobData);

            if (returnSaS)
            {
                return this.GetKeyContainerSas(blobContainerName, sasExpireTimeSpan);
            }

            return null;
        }

        public string UploadBlob(
            string blobContainerName,
            string blobName,
            byte[] blobData,
            int sasExpireInHours = 0,
            bool deleteIfExists = true,
            bool returnSaS = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
            ArgumentNullException.ThrowIfNull(blobData, nameof(blobData));

            if (!blobData.Any())
            {
                throw new ArgumentNullException("Value of the argument was not provided.", nameof(blobData));
            }

            if ((returnSaS) && (sasExpireInHours <= 0))
            {
                throw new ArgumentNullException("Value of the argument should be greater than zero.", nameof(sasExpireInHours));
            }

            return this.UploadBlob(blobContainerName, blobName, blobData, TimeSpan.FromHours(sasExpireInHours), deleteIfExists, returnSaS);
        }

        public string UploadBlobReturnSaS(string blobContainerName, string blobName, byte[] blobData, int sasExpireInHours, bool deleteIfExists = true)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

            return this.UploadBlob(blobContainerName, blobName, blobData, sasExpireInHours, deleteIfExists, returnSaS: true);
        }

        public void UploadBlob(BlobContainerClient blobContainerClient, string blobName, byte[] blobData)
        {
            ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            MSAzure.Response<bool> blobDeleted = blobClient.DeleteIfExists();

            MSAzure.Response<BlobContentInfo> uploadedBlobContent = blobClient.Upload(new BinaryData(blobData));
            if ((uploadedBlobContent == null) || (uploadedBlobContent.Value == null))
            {
                throw new InvalidOperationException($"Unable to upload blob of name {blobName} in blob container {blobContainerClient.Name}.");
            }
        }

        public void UploadBlob(BlobContainerClient blobContainerClient, string blobName,  FileStream fileStream)
        {
            ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
            ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            MSAzure.Response<bool> blobDeleted = blobClient.DeleteIfExists();

            MSAzure.Response<BlobContentInfo> uploadedBlobContent = blobClient.Upload(fileStream);
            if ((uploadedBlobContent == null) || (uploadedBlobContent.Value == null))
            {
                throw new InvalidOperationException($"Unable to upload blob of name {blobName} in blob container {blobContainerClient.Name}.");
            }
        }

        public void DownloadBlob(BlobContainerClient blobContainerClient, string blobName, Stream outputStream)
        {
            ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));

            byte[] blobData = blobContainerClient.DownloadBlobBytes(blobName);
            if ((blobData != null) && (blobData.Any()))
            {
                outputStream.Write(blobData, 0, blobData.Length);
            }
        }

        public bool DeleteContainer(string blobContainerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));

            BlobContainerClient blobContainerClient = new BlobContainerClient(this._storageAccountConnectionString, blobContainerName);
            return blobContainerClient.DeleteIfExists();
        }

        public bool DeleteBlob(string blobContainerName, string blobName)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

            BlobClient blobClient = new BlobClient(this._storageAccountConnectionString, blobContainerName, blobName);
            return blobClient.DeleteIfExists();
        }

        public string GetKeyContainerSas(string blobContainerName, TimeSpan sasExpireFromNow)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerName, nameof(blobContainerName));

            BlobServiceClient blobServiceClient = new BlobServiceClient(this._storageAccountConnectionString);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
            return this.GetKeyContainerSas(blobServiceClient, blobContainerClient, sasExpireFromNow);
        }

        public string GetKeyContainerSas(BlobContainerClient blobContainerClient, TimeSpan sasExpireFromNow)
        {
            ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));

            BlobServiceClient blobServiceClient = new BlobServiceClient(this._storageAccountConnectionString);
            return this.GetKeyContainerSas(blobServiceClient, blobContainerClient, sasExpireFromNow);
        }

        public List<string> GetBlobNamesFromContainerSas(string blobContainerSAS)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerSAS, nameof(blobContainerSAS));

            BlobContainerClient blobContainerClient = new BlobContainerClient(new Uri(blobContainerSAS));
            List<BlobItem> allBlobs = blobContainerClient.GetAllBlobs();

            return allBlobs.Select(b => b.Name).ToList();
        }

        public void DownloadBlobToFileFromContainerSas(string blobContainerSAS, string blobName, string filePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(blobContainerSAS, nameof(blobContainerSAS));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            BlobContainerClient blobContainerClient = new BlobContainerClient(new Uri(blobContainerSAS));
            this.DownloadBlobToFile(blobContainerClient, blobName, filePath);
        }

        public string GetKeyContainerSas(
            BlobServiceClient blobServiceClient, 
            BlobContainerClient blobContainerClient, 
            TimeSpan sasExpireFromNow)
        {
            ArgumentNullException.ThrowIfNull(blobServiceClient, nameof(blobServiceClient));
            ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));

            // Create a SAS token that's also valid for seven days.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobContainerClient.Name,
                Resource = "c",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.Add(sasExpireFromNow)
            };

            // Specify racwl permissions for the SAS.
            sasBuilder.SetPermissions(BlobContainerSasPermissions.All);

            return blobContainerClient.GenerateSasUri(sasBuilder).ToString();
        }

        public string GetBlobSas(string blobContainerName, string blobName, TimeSpan expiresIn)
        {
            ArgumentNullException.ThrowIfNull(blobContainerName, nameof(blobContainerName));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

            return this.GetBlobSas(new BlobClient(this._storageAccountConnectionString, blobContainerName, blobName), expiresIn);
        }

        public string GetBlobSas(BlobClient blobClient, TimeSpan expiresIn)
        {
            ArgumentNullException.ThrowIfNull(blobClient, nameof(blobClient));

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
            };

            // Specify read and write permissions for the SAS.
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Delete);

            string blobUri = blobClient.GenerateSasUri(sasBuilder).ToString();
            return blobUri;
        }

        public BlobContainerClient GetBlobContainerClientByURI(string uri)
        {
            ArgumentException.ThrowIfNullOrEmpty(uri, nameof(uri));

            return new BlobContainerClient(new Uri(uri));
        }

        public BlobContainerClient GetBlobContainerClientByName(string containerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

            return new BlobContainerClient(this._storageAccountConnectionString, containerName);
        }

        public void UploadBlobFromFile(BlobContainerClient blobContainerClient, string blobName, string filePath)
        {
            ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            byte[] allFileData = File.ReadAllBytes(filePath);
            this.UploadBlob(blobContainerClient, blobName, allFileData);
        }

        public void DownloadBlobToFile(BlobContainerClient blobContainerClient, string blobName, string filePath)
        {
            ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));
            ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            byte[] allBlobData = blobContainerClient.DownloadBlobBytes(blobName);
            File.WriteAllBytes(filePath, allBlobData);
        }
    }
}
