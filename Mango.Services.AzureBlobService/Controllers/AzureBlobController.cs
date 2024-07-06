using Mango.Services.AzureBlobService.Models;
using Mango.Services.AzureBlobService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AzureBlobService.Controllers;

[ApiController]
[Route("api/AzureBlob")]
public class AzureBlobController : ControllerBase
{
    private readonly IAzureBlobImageUploadRepository _azureBlobImageUploadRepository;

    public AzureBlobController(IAzureBlobImageUploadRepository azureBlobImageUploadRepository)
    {
        _azureBlobImageUploadRepository = azureBlobImageUploadRepository;
    }
    
    [HttpPost("Upload")]
    [Authorize]
    public async Task<ResponseDto> Upload(IFormFile file)
    {
        var response = new ResponseDto();
        
        if (file == null || file.Length == 0)
        {
            response.IsSuccess = false;
            response.DisplayMessage = "File is undefined";
            return response;
        }
        
        try
        {
            string url = await _azureBlobImageUploadRepository.UploadImageAsync(file);
            response.Result = url;
        }
        catch (Exception e)
        {
            response.IsSuccess = false;
            response.ErrorMessages = new List<string>() { e.ToString() };
        }

        return response;
    }
}