using ECommerce.BLL.DTOs.Cart;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class CartController(IProductService productService, ICartService cartService) : Controller
{
    public IActionResult Index()
    {
        return View(new CartIndexViewModel
        {
            Items = cartService.GetCart()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, string? returnUrl = null)
    {
        if (id <= 0)
        {
            TempData["ErrorMessage"] = "The selected product is invalid.";
            return RedirectToLocal(returnUrl);
        }

        var product = await productService.GetByIdAsync(id);
        if (product is null)
        {
            TempData["ErrorMessage"] = "The requested product was not found.";
            return RedirectToLocal(returnUrl);
        }

        cartService.AddItem(new CartItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Price = product.Price,
            Quantity = 1,
            ImageUrl = product.ImageUrl
        });

        TempData["SuccessMessage"] = $"{product.Name} was added to your cart.";
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        if (!cartService.RemoveItem(id))
        {
            TempData["ErrorMessage"] = "The selected cart item could not be removed.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Item removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Increase(int id)
    {
        if (!cartService.IncreaseQuantity(id))
        {
            TempData["ErrorMessage"] = "The selected cart item could not be updated.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Decrease(int id)
    {
        if (!cartService.DecreaseQuantity(id))
        {
            TempData["ErrorMessage"] = "The selected cart item could not be updated.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        cartService.ClearCart();
        TempData["SuccessMessage"] = "Your cart has been cleared.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
