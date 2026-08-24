using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Orders;

namespace ECommerce.BLL.Services.Interfaces;

public interface IOrderService
{
    Task<decimal?> GetCheckoutTotalAsync();

    Task<OperationResult> FinalizeCheckoutAsync(
        string userId,
        string deliveryAddress,
        string phone,
        decimal expectedTotal,
        string? paymentIntentId);

    Task<IReadOnlyList<OrderSummaryDto>> GetUserOrdersAsync(string userId);

    Task<OrderDetailsDto?> GetUserOrderDetailsAsync(int id, string userId);

    Task<IReadOnlyList<OrderSummaryDto>> GetAllAsync();

    Task<OrderDetailsDto?> GetByIdAsync(int id);
}
