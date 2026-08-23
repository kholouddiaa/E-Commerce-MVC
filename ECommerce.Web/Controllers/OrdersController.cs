using System.Security.Claims;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

[Authorize]
public class OrdersController(IOrderService orderService, ICartService cartService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var orders = await orderService.GetUserOrdersAsync(userId);
        return View(orders);
    }

    public IActionResult Checkout()
    {
        var cartItems = cartService.GetCart();
        if (cartItems.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        return View(new CheckoutViewModel { Items = cartItems });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel viewModel)
    {
        var cartItems = cartService.GetCart();
        if (cartItems.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Items = cartItems;
            return View(viewModel);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var result = await orderService.CheckoutAsync(userId, viewModel.DeliveryAddress, viewModel.Phone);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to complete your order.");
            viewModel.Items = cartItems;
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Your payment was successful and your order has been placed.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var order = await orderService.GetUserOrderDetailsAsync(id, userId);
        return order is null ? NotFound() : View(order);
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
