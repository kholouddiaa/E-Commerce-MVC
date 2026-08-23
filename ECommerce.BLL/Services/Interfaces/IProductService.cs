using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Products;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BLL.Services.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync();

    Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParameters queryParameters);

    Task<ProductDto?> GetByIdAsync(int id);

    Task<IReadOnlyList<ProductDto>> GetArchivedAsync();

    Task<ProductUpsertDto?> GetForEditAsync(int id);

    Task<int> GetTotalCountAsync();

    Task<IReadOnlyDictionary<int, int>> GetCategoryProductCountsAsync();

    Task<OperationResult> CreateAsync(ProductUpsertDto productDto, IFormFile? imageFile);

    Task<OperationResult> UpdateAsync(ProductUpsertDto productDto, IFormFile? imageFile);

    Task<OperationResult> DeleteAsync(int id);

    Task<OperationResult> RestoreAsync(int id);
}
