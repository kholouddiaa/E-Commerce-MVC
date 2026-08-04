using ECommerce.BLL.DTOs.Cart;

namespace ECommerce.Web.ViewModels;

public class CartIndexViewModel
{
    public IReadOnlyList<CartItem> Items { get; set; } = [];

    public bool HasItems => Items.Count > 0;

    public decimal OrderTotal => Items.Sum(item => item.Price * item.Quantity);
}
