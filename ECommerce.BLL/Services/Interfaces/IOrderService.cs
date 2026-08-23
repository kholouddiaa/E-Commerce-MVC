using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Orders;

namespace ECommerce.BLL.Services.Interfaces;

public interface IOrderService
{
    Task<OperationResult> CheckoutAsync(string userId, string deliveryAddress, string phone);

    Task<IReadOnlyList<OrderSummaryDto>> GetUserOrdersAsync(string userId);

    Task<OrderDetailsDto?> GetUserOrderDetailsAsync(int id, string userId);

    Task<IReadOnlyList<OrderSummaryDto>> GetAllAsync();

    Task<OrderDetailsDto?> GetByIdAsync(int id);
}
