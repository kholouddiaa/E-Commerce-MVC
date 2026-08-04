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

        foreach (var productImageRootPath in GetProductImageRootPaths())
        {
            var normalizedProductImageRootPath =
                Path.GetFullPath(productImageRootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var candidatePath = Path.GetFullPath(Path.Combine(productImageRootPath, imageFileName));

            if (!candidatePath.StartsWith(normalizedProductImageRootPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                if (File.Exists(candidatePath))
                {
                    File.Delete(candidatePath);
                    return true;
                }
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

        return true;
    }

    private string GetConfiguredProductImageRootPath()
    {
        var webRootPath = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        return Path.Combine(webRootPath, "uploads", "products");
    }

    private IReadOnlyList<string> GetProductImageRootPaths()
    {
        var configuredWebRootPath = GetConfiguredProductImageRootPath();
        var contentRootProductImagePath = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot", "uploads", "products");

        return new[] { configuredWebRootPath, contentRootProductImagePath }
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? GetProductImageFileName(string imagePath)
    {
        var normalizedPath = imagePath.Trim();
        if (Uri.TryCreate(normalizedPath, UriKind.Absolute, out var absoluteUri))
        {
            normalizedPath = absoluteUri.AbsolutePath;
        }

        normalizedPath = normalizedPath
            .TrimStart('~')
            .TrimStart('/', '\\')
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        var normalizedProductImageFolder = ProductImageFolder.Replace('/', Path.DirectorySeparatorChar);
        var expectedFolderPrefix = normalizedProductImageFolder + Path.DirectorySeparatorChar;

        if (!normalizedPath.StartsWith(expectedFolderPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var folderIndex = normalizedPath.IndexOf(expectedFolderPrefix, StringComparison.OrdinalIgnoreCase);
            if (folderIndex < 0)
            {
                return null;
            }

            normalizedPath = normalizedPath[folderIndex..];
        }

        var imageFileName = Path.GetFileName(normalizedPath);
        return string.IsNullOrWhiteSpace(imageFileName) ? null : imageFileName;
    }
}
