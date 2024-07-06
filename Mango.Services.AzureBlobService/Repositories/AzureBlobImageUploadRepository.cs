using Azure.Storage.Blobs;

namespace Mango.Services.AzureBlobService.Repositories;

public class AzureBlobImageUploadRepository : IAzureBlobImageUploadRepository
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public AzureBlobImageUploadRepository(string connectionString, string containerName)
    {
        _connectionString = connectionString;
        _containerName = containerName;
    }

    public async Task<string> UploadImageAsync(IFormFile image)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);

        // Get the Blob container client
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        // Ensure the container exists
        await containerClient.CreateIfNotExistsAsync();

        // Get the Blob client
        var blobClient = containerClient.GetBlobClient(image.FileName);

        // Upload the file
        using (var stream = image.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, true);
        }

        // Return the URI of the uploaded blob
        return blobClient.Uri.ToString();
    }
}