using System.ComponentModel.DataAnnotations;
using ECommerce.BLL.DTOs.Cart;

namespace ECommerce.Web.ViewModels;

public class CheckoutViewModel
{
    [Required]
    [StringLength(500)]
    [Display(Name = "Delivery Address")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    public IReadOnlyList<CartItem> Items { get; set; } = [];

    public string StripePublishableKey { get; set; } = string.Empty;

    public decimal OrderTotal => Items.Sum(item => item.Price * item.Quantity);
}
