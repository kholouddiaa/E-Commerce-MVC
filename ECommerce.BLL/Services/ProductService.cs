using AutoMapper;
using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Products;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.DAL.Entities;
using ECommerce.DAL.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BLL.Services;

public class ProductService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
    {
        var products = await unitOfWork.Products.GetAllWithCategoryAsync();
        return mapper.Map<IReadOnlyList<ProductDto>>(products);
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParameters queryParameters)
    {
        var normalizedQueryParameters = NormalizeQueryParameters(queryParameters);

        var (products, totalItems, currentPage) = await unitOfWork.Products.GetPagedWithCategoryAsync(
            normalizedQueryParameters.SearchTerm,
            normalizedQueryParameters.CategoryId,
            normalizedQueryParameters.SortOrder,
            normalizedQueryParameters.PageNumber,
            normalizedQueryParameters.PageSize);

        return new PagedResult<ProductDto>
        {
            Items = mapper.Map<IReadOnlyList<ProductDto>>(products),
            CurrentPage = currentPage,
            PageSize = normalizedQueryParameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await unitOfWork.Products.GetByIdWithCategoryAsync(id);
        return product is null ? null : mapper.Map<ProductDto>(product);
    }

    public async Task<ProductUpsertDto?> GetForEditAsync(int id)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        return product is null ? null : mapper.Map<ProductUpsertDto>(product);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await unitOfWork.Products.GetTotalCountAsync();
    }

    public async Task<IReadOnlyDictionary<int, int>> GetCategoryProductCountsAsync()
    {
        return await unitOfWork.Products.GetCategoryProductCountsAsync();
    }

    public async Task<OperationResult> CreateAsync(ProductUpsertDto productDto, IFormFile? imageFile)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(productDto.CategoryId);
        if (category is null)
        {
            return OperationResult.Failure("The selected category is invalid.");
        }

        var imageValidationResult = fileService.ValidateProductImage(imageFile, imageRequired: true);
        if (!imageValidationResult.Succeeded)
        {
            return imageValidationResult;
        }

        string? uploadedImageUrl;
        try
        {
            uploadedImageUrl = await fileService.UploadImageAsync(imageFile!);
        }
        catch
        {
            return OperationResult.Failure("Unable to save the product image.", "ImageFile");
        }

        productDto.ImageUrl = uploadedImageUrl;

        try
        {
            var product = mapper.Map<Product>(productDto);
            TrimProduct(product);

            await unitOfWork.Products.AddAsync(product);
            await unitOfWork.SaveChangesAsync();

            return OperationResult.Success();
        }
        catch
        {
            fileService.DeleteImage(uploadedImageUrl);
            return OperationResult.Failure("Unable to create the product.");
        }
    }

    public async Task<OperationResult> UpdateAsync(ProductUpsertDto productDto, IFormFile? imageFile)
    {
        var existingProduct = await unitOfWork.Products.GetByIdAsync(productDto.Id);
        if (existingProduct is null)
        {
            return OperationResult.Failure("The requested product was not found.");
        }

        var category = await unitOfWork.Categories.GetByIdAsync(productDto.CategoryId);
        if (category is null)
        {
            return OperationResult.Failure("The selected category is invalid.");
        }

        productDto.ImageUrl = existingProduct.ImageUrl;

        var imageValidationResult = fileService.ValidateProductImage(
            imageFile,
            imageRequired: string.IsNullOrWhiteSpace(existingProduct.ImageUrl));

        if (!imageValidationResult.Succeeded)
        {
            return imageValidationResult;
        }

        var previousImageUrl = existingProduct.ImageUrl;
        string? uploadedImageUrl = null;

        if (imageFile is not null)
        {
            try
            {
                uploadedImageUrl = await fileService.UploadImageAsync(imageFile);
            }
            catch
            {
                return OperationResult.Failure("Unable to save the product image.", "ImageFile");
            }

            productDto.ImageUrl = uploadedImageUrl;
        }

        mapper.Map(productDto, existingProduct);
        TrimProduct(existingProduct);

        try
        {
            unitOfWork.Products.Update(existingProduct);
            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(uploadedImageUrl))
            {
                fileService.DeleteImage(uploadedImageUrl);
            }

            existingProduct.ImageUrl = previousImageUrl;
            return OperationResult.Failure("Unable to update the product.");
        }

        if (!string.IsNullOrWhiteSpace(uploadedImageUrl) &&
            !string.IsNullOrWhiteSpace(previousImageUrl))
        {
            fileService.DeleteImage(previousImageUrl);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteAsync(int id)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
        {
            return OperationResult.Failure("The requested product was not found.");
        }

        if (!fileService.DeleteImage(product.ImageUrl))
        {
            return OperationResult.Failure("Unable to delete the product image.");
        }

        try
        {
            unitOfWork.Products.Delete(product);
            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            return OperationResult.Failure("Unable to delete the product.");
        }

        return OperationResult.Success();
    }

    private static void TrimProduct(Product product)
    {
        product.Name = product.Name.Trim();
        product.Description = product.Description?.Trim();
        product.ImageUrl = product.ImageUrl?.Trim();
    }

    private static ProductQueryParameters NormalizeQueryParameters(ProductQueryParameters queryParameters)
    {
        return new ProductQueryParameters
        {
            SearchTerm = string.IsNullOrWhiteSpace(queryParameters.SearchTerm)
                ? null
                : queryParameters.SearchTerm.Trim(),
            SortOrder = ProductSortOrder.Normalize(queryParameters.SortOrder),
            PageNumber = Math.Max(queryParameters.PageNumber, 1),
            PageSize = Math.Clamp(
                queryParameters.PageSize <= 0
                    ? ProductQueryParameters.DefaultPageSize
                    : queryParameters.PageSize,
                1,
                ProductQueryParameters.MaxPageSize),
            CategoryId = queryParameters.CategoryId is > 0 ? queryParameters.CategoryId : null
        };
    }
}
