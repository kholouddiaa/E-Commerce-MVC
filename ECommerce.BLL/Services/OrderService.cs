using System.Text;
using ECommerce.BLL.Emails;
using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Orders;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.DAL.Entities;
using ECommerce.DAL.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ECommerce.BLL.Services;

public class OrderService(
    IUnitOfWork unitOfWork,
    ICartService cartService,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<decimal?> GetCheckoutTotalAsync()
    {
        var checkoutData = await GetCheckoutDataAsync();
        return checkoutData?.TotalAmount;
    }

    public async Task<OperationResult> FinalizeCheckoutAsync(
        string userId,
        string deliveryAddress,
        string phone,
        decimal expectedTotal,
        string? paymentIntentId)
    {
        var checkoutData = await GetCheckoutDataAsync();
        if (checkoutData is null)
        {
            return OperationResult.Failure("Your cart contains an invalid or unavailable item.");
        }

        if (checkoutData.TotalAmount != expectedTotal)
        {
            return OperationResult.Failure("Your cart changed before payment could be completed. Please try again.");
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            DeliveryAddress = deliveryAddress.Trim(),
            Phone = phone.Trim(),
            Status = "Paid"
        };

        foreach (var cartItem in checkoutData.CartItems)
        {
            var product = checkoutData.ProductsById[cartItem.ProductId];
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity
            });
        }

        order.TotalAmount = checkoutData.TotalAmount;

        try
        {
            await unitOfWork.Orders.AddAsync(order);
            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            return OperationResult.Failure("Unable to complete your order. Please try again.");
        }

        cartService.ClearCart();
        await SendOrderConfirmationAsync(order, paymentIntentId);
        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await unitOfWork.Orders.GetByUserIdAsync(userId);

        return orders.Select(order => new OrderSummaryDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount
        }).ToList();
    }

    public async Task<OrderDetailsDto?> GetUserOrderDetailsAsync(int id, string userId)
    {
        var order = await unitOfWork.Orders.GetByIdAndUserIdAsync(id, userId);
        return order is null ? null : MapOrderDetails(order);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetAllAsync()
    {
        var orders = await unitOfWork.Orders.GetAllAsync();

        return orders.Select(order => new OrderSummaryDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CustomerName = order.User?.FullName,
            CustomerEmail = order.User?.Email
        }).ToList();
    }

    public async Task<OrderDetailsDto?> GetByIdAsync(int id)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(id);
        return order is null ? null : MapOrderDetails(order);
    }

    private static OrderDetailsDto MapOrderDetails(Order order)
    {
        return new OrderDetailsDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            DeliveryAddress = order.DeliveryAddress,
            Phone = order.Phone,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CustomerName = order.User?.FullName,
            CustomerEmail = order.User?.Email,
            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    private async Task<CheckoutData?> GetCheckoutDataAsync()
    {
        var cartItems = cartService.GetCart();
        if (cartItems.Count == 0)
        {
            return null;
        }

        var productIds = cartItems
            .Where(item => item.ProductId > 0 && item.Quantity > 0)
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        if (productIds.Count != cartItems.Count)
        {
            return null;
        }

        var products = await unitOfWork.Products.GetByIdsAsync(productIds);
        if (products.Count != productIds.Count)
        {
            return null;
        }

        var productsById = products.ToDictionary(product => product.Id);
        var totalAmount = cartItems.Sum(item => productsById[item.ProductId].Price * item.Quantity);
        return new CheckoutData(cartItems, productsById, totalAmount);
    }

    private async Task SendOrderConfirmationAsync(Order order, string? paymentIntentId)
    {
        try
        {
            var user = await userManager.FindByIdAsync(order.UserId);
            if (string.IsNullOrWhiteSpace(user?.Email))
            {
                return;
            }

            var emailBody = BuildOrderConfirmationEmail(order, paymentIntentId);
            await emailService.SendHtmlEmailAsync(user.Email, $"Order #{order.Id} Confirmation", emailBody);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to send order confirmation email for order {OrderId}.", order.Id);
        }
    }

    private static string BuildOrderConfirmationEmail(Order order, string? paymentIntentId)
    {
        var items = new StringBuilder();
        foreach (var item in order.Items)
        {
            items.Append($"<tr><td style=\"padding:8px;border-bottom:1px solid #e1e5e8;\">{EmailTemplate.Encode(item.ProductName)}</td><td style=\"padding:8px;border-bottom:1px solid #e1e5e8;\">{item.Quantity}</td><td style=\"padding:8px;border-bottom:1px solid #e1e5e8;\">{item.UnitPrice:C}</td></tr>");
        }

        var deliveryAddress = EmailTemplate.Encode(order.DeliveryAddress)
            .Replace("\r\n", "<br />")
            .Replace("\n", "<br />");

        var paymentReference = string.IsNullOrWhiteSpace(paymentIntentId)
            ? string.Empty
            : $"<br /><strong>Payment Reference:</strong> {EmailTemplate.Encode(paymentIntentId)}";

        return EmailTemplate.Create(
            "Order Confirmation",
            "Your Order Is Confirmed",
            $"<p>Thank you for your order.</p><p><strong>Order:</strong> #{order.Id}<br /><strong>Payment Status:</strong> {EmailTemplate.Encode(order.Status)}<br /><strong>Payment Method:</strong> Stripe{paymentReference}<br /><strong>Order Date:</strong> {order.OrderDate.ToLocalTime():f}<br /><strong>Delivery Address:</strong><br />{deliveryAddress}</p><table style=\"width:100%;border-collapse:collapse;\"><thead><tr><th style=\"text-align:left;padding:8px;border-bottom:2px solid #e1e5e8;\">Product</th><th style=\"text-align:left;padding:8px;border-bottom:2px solid #e1e5e8;\">Quantity</th><th style=\"text-align:left;padding:8px;border-bottom:2px solid #e1e5e8;\">Unit Price</th></tr></thead><tbody>{items}</tbody></table><p style=\"text-align:right;\"><strong>Total Amount: {order.TotalAmount:C}</strong></p>");
    }

    private sealed record CheckoutData(
        IReadOnlyList<ECommerce.BLL.DTOs.Cart.CartItem> CartItems,
        IReadOnlyDictionary<int, Product> ProductsById,
        decimal TotalAmount);
}
