using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureLib;

public class AzureSaSClient
{
    private string _blobContainerSaS;

    public AzureSaSClient(string blobContainerSaS)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobContainerSaS, nameof(blobContainerSaS));

        this._blobContainerSaS = blobContainerSaS;
    }

    public byte[] DownloadBlobBytes(string blobName)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

        return new BlobContainerClient(new Uri(this._blobContainerSaS)).DownloadBlobBytes(blobName);
    }

    public void DownloadBlobToFile(string blobName, string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

        new BlobContainerClient(new Uri(this._blobContainerSaS)).DownloadBlobToFile(blobName, filePath);
    }

    public List<string> GetAllBlobNames()
    {
        List<BlobItem> blobItems = new BlobContainerClient(new Uri(this._blobContainerSaS)).GetAllBlobs();
        return blobItems.Select(b => b.Name).ToList();
    }
}
