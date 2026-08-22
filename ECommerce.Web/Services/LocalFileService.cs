using ECommerce.BLL.Common;
using ECommerce.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Web.Services;

public class LocalFileService(IWebHostEnvironment webHostEnvironment) : IFileService
{
    private const long MaxImageSizeInBytes = 2 * 1024 * 1024;
    private const string ProductImageFolder = "uploads/products";
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public OperationResult ValidateProductImage(IFormFile? file, bool imageRequired)
    {
        if (file is null)
        {
            return imageRequired
                ? OperationResult.Failure("Product image is required.", "ImageFile")
                : OperationResult.Success();
        }

        if (file.Length == 0)
        {
            return OperationResult.Failure("The selected image file is empty.", "ImageFile");
        }

        if (file.Length > MaxImageSizeInBytes)
        {
            return OperationResult.Failure("The image size must be 2 MB or less.", "ImageFile");
        }

        var fileExtension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(fileExtension) ||
            !AllowedImageExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult.Failure("Please upload a JPG, JPEG, PNG, or WEBP image.", "ImageFile");
        }

        return OperationResult.Success();
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var productImageRootPath = GetConfiguredProductImageRootPath();
        Directory.CreateDirectory(productImageRootPath);

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{fileExtension}";
        var filePath = Path.Combine(productImageRootPath, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        return $"/{ProductImageFolder}/{fileName}";
    }

    public bool DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return true;
        }

        var imageFileName = GetProductImageFileName(imagePath);
        if (string.IsNullOrWhiteSpace(imageFileName))
        {
            return false;
        }

        var productImageRootPath = GetConfiguredProductImageRootPath();
        var candidatePath = Path.Combine(productImageRootPath, imageFileName);

        try
        {
            if (File.Exists(candidatePath))
            {
                File.Delete(candidatePath);
            }

            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private string GetConfiguredProductImageRootPath()
    {
        var webRootPath = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        return Path.Combine(webRootPath, "uploads", "products");
    }

    private static string? GetProductImageFileName(string imagePath)
    {
        var normalizedPath = imagePath.Trim().Replace('\\', '/');
        var imageFileName = Path.GetFileName(normalizedPath);

        return string.IsNullOrWhiteSpace(imageFileName) ? null : imageFileName;
    }
}
