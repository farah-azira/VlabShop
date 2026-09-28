using Microsoft.AspNetCore.Mvc.Rendering;
using VlabShop.Web.Models;

namespace VlabShop.Web.ViewModels.Catalogue;

public class CatalogueIndexViewModel
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];

    public IReadOnlyList<Product> Products { get; set; } = [];
}
