using ECommerce.BLL.DTOs.Cart;

namespace ECommerce.BLL.Services.Interfaces;

public interface ICartService
{
    IReadOnlyList<CartItem> GetCart();

    void AddItem(CartItem item);

    bool RemoveItem(int productId);

    bool IncreaseQuantity(int productId);

    bool DecreaseQuantity(int productId);

    void ClearCart();
}
