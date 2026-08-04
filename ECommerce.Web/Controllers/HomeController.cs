using System.Diagnostics;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.Helpers;
using ECommerce.Web.Models;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class HomeController(IProductService productService, ICategoryService categoryService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await productService.GetAllAsync();
        var categories = await categoryService.GetAllAsync();

        var model = new StorefrontHomeViewModel
        {
            Categories = StorefrontViewModelFactory.BuildCategories(categories, products)
                .Take(6)
                .ToList(),
            FeaturedProducts = products.Take(8).ToList(),
            NewArrivals = products
                .OrderByDescending(product => product.Id)
                .Take(8)
                .ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
