using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using MSAzure = Azure;

namespace AzureLib;

public static class BlobContainerClientExtensions
{
    public static byte[] DownloadBlobBytes(this BlobContainerClient blobContainerClient, string blobName)
    {
        ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));
        ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
        MSAzure.Response<bool> blobExists = blobClient.Exists();
        if ((blobExists == null) || (!blobExists.Value))
        {
            throw new InvalidOperationException($"The blob of name {blobName} does not exist in the blob container {blobContainerClient.Name}.");
        }

        MSAzure.Response<BlobDownloadResult> blobDownloadResult = blobClient.DownloadContent();
        if ((blobDownloadResult == null) || (blobDownloadResult.Value == null))
        {
            throw new InvalidOperationException($"Unable to download the blob of name {blobName} from the blob container {blobContainerClient.Name}.");
        }

        return blobDownloadResult.Value.Content.ToArray();
    }

    public static void DownloadBlobToFile(this BlobContainerClient blobContainerClient, string blobName, string filePath)
    {
        ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));
        ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

        byte[] allBlobData = blobContainerClient.DownloadBlobBytes(blobName);
        File.WriteAllBytes(filePath, allBlobData);
    }

    public static List<BlobItem> GetAllBlobs(this BlobContainerClient blobContainerClient)
    {
        ArgumentNullException.ThrowIfNull(blobContainerClient, nameof(blobContainerClient));

        List<BlobItem> allBlobItems = new List<BlobItem>();

        MSAzure.Pageable<BlobItem> pageableBlobs = blobContainerClient.GetBlobs(BlobTraits.All, BlobStates.All);
        foreach (MSAzure.Page<BlobItem> blobPage in pageableBlobs.AsPages())
        {
            foreach (BlobItem blobItem in blobPage.Values)
            {
                allBlobItems.Add(blobItem);
            }
        }

        return allBlobItems;
    }
}
