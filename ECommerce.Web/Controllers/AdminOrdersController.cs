using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminOrdersController(IOrderService orderService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orders = await orderService.GetAllAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order is null ? NotFound() : View(order);
    }
}
