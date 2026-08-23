using ECommerce.BLL.DTOs.Categories;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class CategoriesController(ICategoryService categoryService, IProductService productService) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var categories = await categoryService.GetAllAsync();
        var products = await productService.GetAllAsync();
        var model = StorefrontViewModelFactory.BuildCategories(categories, products);

        return View(model);
    }

    public async Task<IActionResult> Admin()
    {
        var categories = await categoryService.GetAllAsync();
        return View(categories);
    }

    public async Task<IActionResult> Details(int id)
    {
        var category = await categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : View(category);
    }

    public IActionResult Create()
    {
        return View(new CategoryUpsertDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryUpsertDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return View(categoryDto);
        }

        var result = await categoryService.CreateAsync(categoryDto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create the category.");
            return View(categoryDto);
        }

        TempData["SuccessMessage"] = "Category created successfully.";
        return RedirectToAction(nameof(Admin));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await categoryService.GetForEditAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryUpsertDto categoryDto)
    {
        if (id != categoryDto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(categoryDto);
        }

        var result = await categoryService.UpdateAsync(categoryDto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update the category.");
            return View(categoryDto);
        }

        TempData["SuccessMessage"] = "Category updated successfully.";
        return RedirectToAction(nameof(Admin));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await categoryService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Unable to archive the category.";
            return RedirectToAction(nameof(Admin));
        }

        TempData["SuccessMessage"] = "Category archived successfully.";
        return RedirectToAction(nameof(Admin));
    }
}
