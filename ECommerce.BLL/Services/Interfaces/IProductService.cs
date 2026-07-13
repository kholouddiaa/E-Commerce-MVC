using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Products;

namespace ECommerce.BLL.Services.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync();

    Task<ProductDto?> GetByIdAsync(int id);

    Task<ProductUpsertDto?> GetForEditAsync(int id);

    Task<OperationResult> CreateAsync(ProductUpsertDto productDto);

    Task<OperationResult> UpdateAsync(ProductUpsertDto productDto);

    Task<OperationResult> DeleteAsync(int id);
}
