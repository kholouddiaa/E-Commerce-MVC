using AutoMapper;
using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Categories;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.DAL.Entities;
using ECommerce.DAL.UnitOfWork.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.BLL.Services;

public class CategoryService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMemoryCache memoryCache) : ICategoryService
{
    private const string CategoriesCacheKey = "categories";

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        if (memoryCache.TryGetValue(CategoriesCacheKey, out IReadOnlyList<CategoryDto>? cachedCategories) &&
            cachedCategories is not null)
        {
            return cachedCategories;
        }

        var categories = await unitOfWork.Categories.GetAllAsync();
        var mappedCategories = mapper.Map<IReadOnlyList<CategoryDto>>(categories);

        memoryCache.Set(CategoriesCacheKey, mappedCategories, TimeSpan.FromMinutes(30));

        return mappedCategories;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        return category is null ? null : mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryUpsertDto?> GetForEditAsync(int id)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        return category is null ? null : mapper.Map<CategoryUpsertDto>(category);
    }

    public async Task<OperationResult> CreateAsync(CategoryUpsertDto categoryDto)
    {
        if (await unitOfWork.Categories.ExistsByNameAsync(categoryDto.Name))
        {
            return OperationResult.Failure("A category with the same name already exists.");
        }

        var category = mapper.Map<Category>(categoryDto);
        category.Name = category.Name.Trim();
        category.Description = category.Description?.Trim();

        await unitOfWork.Categories.AddAsync(category);
        await unitOfWork.SaveChangesAsync();
        InvalidateCategoriesCache();

        return OperationResult.Success();
    }

    public async Task<OperationResult> UpdateAsync(CategoryUpsertDto categoryDto)
    {
        var existingCategory = await unitOfWork.Categories.GetByIdAsync(categoryDto.Id);
        if (existingCategory is null)
        {
            return OperationResult.Failure("The requested category was not found.");
        }

        if (await unitOfWork.Categories.ExistsByNameAsync(categoryDto.Name, categoryDto.Id))
        {
            return OperationResult.Failure("A category with the same name already exists.");
        }

        mapper.Map(categoryDto, existingCategory);
        existingCategory.Name = existingCategory.Name.Trim();
        existingCategory.Description = existingCategory.Description?.Trim();

        unitOfWork.Categories.Update(existingCategory);
        await unitOfWork.SaveChangesAsync();
        InvalidateCategoriesCache();

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteAsync(int id)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        if (category is null)
        {
            return OperationResult.Failure("The requested category was not found.");
        }

        if (await unitOfWork.Categories.HasProductsAsync(id))
        {
            return OperationResult.Failure("This category cannot be deleted because it is assigned to one or more products.");
        }

        unitOfWork.Categories.Delete(category);
        await unitOfWork.SaveChangesAsync();
        InvalidateCategoriesCache();

        return OperationResult.Success();
    }

    private void InvalidateCategoriesCache()
    {
        memoryCache.Remove(CategoriesCacheKey);
    }
}
