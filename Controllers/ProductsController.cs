using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VlabShop.Web.Data;
using VlabShop.Web.Models;
using VlabShop.Web.ViewModels.Products;

namespace VlabShop.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class ProductsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .OrderBy(product => product.Name)
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .SingleOrDefaultAsync(product => product.Id == id);

        return product is null ? NotFound() : View(product);
    }

    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel
        {
            Categories = await GetCategoryOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        await ValidateCategoryAsync(model.CategoryId);

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryOptionsAsync(model.CategoryId);
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            StockQuantity = model.StockQuantity,
            ImageUrl = model.ImageUrl,
            CategoryId = model.CategoryId,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == id);

        return product is null ? NotFound() : View(await ToFormModelAsync(product));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        await ValidateCategoryAsync(model.CategoryId);

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryOptionsAsync(model.CategoryId);
            return View(model);
        }

        var product = await context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.StockQuantity = model.StockQuantity;
        product.ImageUrl = model.ImageUrl;
        product.CategoryId = model.CategoryId;
        product.IsActive = model.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .SingleOrDefaultAsync(product => product.Id == id);

        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoryOptionsAsync(int? selectedCategoryId = null)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name,
                Selected = category.Id == selectedCategoryId
            })
            .ToListAsync();
    }

    private async Task ValidateCategoryAsync(int categoryId)
    {
        var categoryExists = await context.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Id == categoryId && category.IsActive);

        if (!categoryExists)
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.CategoryId), "Select an active category.");
        }
    }

    private async Task<ProductFormViewModel> ToFormModelAsync(Product product)
    {
        return new ProductFormViewModel
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive,
            Categories = await GetCategoryOptionsAsync(product.CategoryId)
        };
    }
}
