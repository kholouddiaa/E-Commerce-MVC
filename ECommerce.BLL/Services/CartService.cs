using System.Text.Json;
using ECommerce.BLL.DTOs.Cart;
using ECommerce.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BLL.Services;

public class CartService(IHttpContextAccessor httpContextAccessor) : ICartService
{
    private const string CartSessionKey = "Cart";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public IReadOnlyList<CartItem> GetCart()
    {
        return GetCartItems();
    }

    public void AddItem(CartItem item)
    {
        if (item.ProductId <= 0)
        {
            return;
        }

        var cartItems = GetCartItems();
        var existingItem = cartItems.FirstOrDefault(cartItem => cartItem.ProductId == item.ProductId);
        if (existingItem is not null)
        {
            existingItem.ProductName = item.ProductName;
            existingItem.Price = item.Price;
            existingItem.ImageUrl = item.ImageUrl;
            existingItem.Quantity += Math.Max(1, item.Quantity);
        }
        else
        {
            cartItems.Add(new CartItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = Math.Max(1, item.Quantity),
                ImageUrl = item.ImageUrl
            });
        }

        SaveCart(cartItems);
    }

    public bool RemoveItem(int productId)
    {
        if (productId <= 0)
        {
            return false;
        }

        var cartItems = GetCartItems();
        var removedItemCount = cartItems.RemoveAll(cartItem => cartItem.ProductId == productId);
        if (removedItemCount == 0)
        {
            return false;
        }

        SaveCart(cartItems);
        return true;
    }

    public bool IncreaseQuantity(int productId)
    {
        if (productId <= 0)
        {
            return false;
        }

        var cartItems = GetCartItems();
        var existingItem = cartItems.FirstOrDefault(cartItem => cartItem.ProductId == productId);
        if (existingItem is null)
        {
            return false;
        }

        existingItem.Quantity++;
        SaveCart(cartItems);
        return true;
    }

    public bool DecreaseQuantity(int productId)
    {
        if (productId <= 0)
        {
            return false;
        }

        var cartItems = GetCartItems();
        var existingItem = cartItems.FirstOrDefault(cartItem => cartItem.ProductId == productId);
        if (existingItem is null)
        {
            return false;
        }

        existingItem.Quantity--;
        if (existingItem.Quantity <= 0)
        {
            cartItems.Remove(existingItem);
        }

        SaveCart(cartItems);
        return true;
    }

    public void ClearCart()
    {
        GetSession()?.Remove(CartSessionKey);
    }

    private List<CartItem> GetCartItems()
    {
        var session = GetSession();
        if (session is null)
        {
            return [];
        }

        var cartJson = session.GetString(CartSessionKey);
        if (string.IsNullOrWhiteSpace(cartJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson, SerializerOptions) ?? [];
        }
        catch (JsonException)
        {
            session.Remove(CartSessionKey);
            return [];
        }
    }

    private void SaveCart(List<CartItem> cartItems)
    {
        var session = GetSession();
        if (session is null)
        {
            return;
        }

        if (cartItems.Count == 0)
        {
            session.Remove(CartSessionKey);
            return;
        }

        var cartJson = JsonSerializer.Serialize(cartItems, SerializerOptions);
        session.SetString(CartSessionKey, cartJson);
    }

    private ISession? GetSession()
    {
        return httpContextAccessor.HttpContext?.Session;
    }
}
