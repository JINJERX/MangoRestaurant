namespace Mango.Services.AzureBlobService.Repositories;

public interface IAzureBlobImageUploadRepository
{
    public Task<string> UploadImageAsync(IFormFile image);
}