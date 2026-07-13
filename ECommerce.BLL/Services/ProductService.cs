using AutoMapper;
using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Products;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.DAL.Entities;
using ECommerce.DAL.UnitOfWork.Interfaces;

namespace ECommerce.BLL.Services;

public class ProductService(IUnitOfWork unitOfWork, IMapper mapper) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
    {
        var products = await unitOfWork.Products.GetAllWithCategoryAsync();
        return mapper.Map<IReadOnlyList<ProductDto>>(products);
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

    public async Task<OperationResult> CreateAsync(ProductUpsertDto productDto)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(productDto.CategoryId);
        if (category is null)
        {
            return OperationResult.Failure("The selected category is invalid.");
        }

        var product = mapper.Map<Product>(productDto);
        product.Name = product.Name.Trim();
        product.Description = product.Description?.Trim();

        await unitOfWork.Products.AddAsync(product);
        await unitOfWork.SaveChangesAsync();

        return OperationResult.Success();
    }

    public async Task<OperationResult> UpdateAsync(ProductUpsertDto productDto)
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

        mapper.Map(productDto, existingProduct);
        existingProduct.Name = existingProduct.Name.Trim();
        existingProduct.Description = existingProduct.Description?.Trim();

        unitOfWork.Products.Update(existingProduct);
        await unitOfWork.SaveChangesAsync();

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteAsync(int id)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
        {
            return OperationResult.Failure("The requested product was not found.");
        }

        unitOfWork.Products.Delete(product);
        await unitOfWork.SaveChangesAsync();

        return OperationResult.Success();
    }
}
