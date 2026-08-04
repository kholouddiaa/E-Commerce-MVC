using ECommerce.BLL.Common;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BLL.Services.Interfaces;

public interface IFileService
{
    OperationResult ValidateProductImage(IFormFile? file, bool imageRequired);

    Task<string> UploadImageAsync(IFormFile file);

    bool DeleteImage(string? imagePath);
}
