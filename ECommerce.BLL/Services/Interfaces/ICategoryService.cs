using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Categories;

namespace ECommerce.BLL.Services.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync();

    Task<CategoryDto?> GetByIdAsync(int id);

    Task<CategoryUpsertDto?> GetForEditAsync(int id);

    Task<OperationResult> CreateAsync(CategoryUpsertDto categoryDto);

    Task<OperationResult> UpdateAsync(CategoryUpsertDto categoryDto);

    Task<OperationResult> DeleteAsync(int id);
}
