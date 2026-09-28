using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VlabShop.Web.Data;
using VlabShop.Web.ViewModels.Catalogue;

namespace VlabShop.Web.Controllers;

public class CatalogueController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        var productsQuery = context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            productsQuery = productsQuery.Where(product => product.Name.Contains(search));
        }

        if (categoryId.HasValue)
        {
            productsQuery = productsQuery.Where(product => product.CategoryId == categoryId.Value);
        }

        var products = await productsQuery
            .OrderBy(product => product.Name)
            .ToListAsync();

        var categories = await context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name,
                Selected = category.Id == categoryId
            })
            .ToListAsync();

        return View(new CatalogueIndexViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Categories = categories,
            Products = products
        });
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
            .SingleOrDefaultAsync(product => product.Id == id && product.IsActive);

        return product is null ? NotFound() : View(product);
    }
}
