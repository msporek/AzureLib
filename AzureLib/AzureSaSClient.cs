using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureLib;

/// <summary>
/// Class represents logic to deal with Azure SaS for downloading data from them and from Azure Blobs. 
/// </summary>
public class AzureSaSClient
{
    private string _blobContainerSaS;

    /// <summary>
    /// Constructor creates a new instance of <see cref="AzureSaSClient"/> class with the SaS of the Blob Container. 
    /// </summary>
    /// 
    /// <param name="blobContainerSaS">Blob Container SaS.</param>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="blobContainerSaS"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="blobContainerSaS"/> is empty string.</exception>
    public AzureSaSClient(string blobContainerSaS)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobContainerSaS, nameof(blobContainerSaS));

        this._blobContainerSaS = blobContainerSaS;
    }

    /// <summary>
    /// Method downloads contents of blob of the given <paramref name="blobName"/> as binary. 
    /// </summary>
    /// 
    /// <param name="blobName">Blob name.</param>
    /// 
    /// <returns>Blob binary contents downloaded.</returns>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="blobName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="blobName"/> is empty string.</exception>
    public byte[] DownloadBlobBytes(string blobName)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));

        return new BlobContainerClient(new Uri(this._blobContainerSaS)).DownloadBlobBytes(blobName);
    }

    /// <summary>
    /// Method downloads contents of blob of the given <paramref name="blobName"/> and saves it on the local drive at path given by 
    /// <paramref name="filePath"/>. 
    /// </summary>
    /// 
    /// <param name="blobName">Blob name.</param>
    /// <param name="filePath">Path to store the blob data at.</param>
    /// 
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="blobName"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="blobName"/> or <paramref name="filePath"/> is empty string.</exception>
    public void DownloadBlobToFile(string blobName, string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName, nameof(blobName));
        ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

        new BlobContainerClient(new Uri(this._blobContainerSaS)).DownloadBlobToFile(blobName, filePath);
    }

    /// <summary>
    /// Method retrieves and returnes names of all blobs from the blob container. 
    /// </summary>
    /// 
    /// <returns>List of all blobs from the blob container. An empty list is returned if no blobs are present in the container.</returns>
    public List<string> GetAllBlobNames()
    {
        List<BlobItem> blobItems = new BlobContainerClient(new Uri(this._blobContainerSaS)).GetAllBlobs();
        return blobItems.Select(b => b.Name).ToList();
    }
}
