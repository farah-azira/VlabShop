using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VlabShop.Web.Data;
using VlabShop.Web.ViewModels.Cart;

namespace VlabShop.Web.Controllers;

public class CartController(ApplicationDbContext context) : Controller
{
    private const string CartSessionKey = "ShopEase.Cart";

    public async Task<IActionResult> Index()
    {
        var storedItems = GetCartItems();
        if (storedItems.Count == 0)
        {
            return View(new CartViewModel());
        }

        var productIds = storedItems.Select(item => item.ProductId).ToArray();
        var products = await context.Products
            .AsNoTracking()
            .Where(product => product.IsActive && productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id);

        var validItems = new List<CartSessionItem>();
        var lines = new List<CartLineViewModel>();

        foreach (var item in storedItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product) || product.StockQuantity < 1)
            {
                continue;
            }

            var quantity = Math.Min(item.Quantity, product.StockQuantity);
            validItems.Add(new CartSessionItem
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });

            lines.Add(new CartLineViewModel
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                AvailableStock = product.StockQuantity,
                ImageUrl = product.ImageUrl
            });
        }

        SetCartItems(validItems);
        return View(new CartViewModel { Items = lines });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CartItemInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["CartError"] = "Quantity must be at least 1.";
            return RedirectToAction("Details", "Catalogue", new { id = model.ProductId });
        }

        var product = await context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == model.ProductId && product.IsActive);

        if (product is null)
        {
            return NotFound();
        }

        var cartItems = GetCartItems();
        var existingItem = cartItems.SingleOrDefault(item => item.ProductId == product.Id);
        var requestedQuantity = (existingItem?.Quantity ?? 0) + model.Quantity;

        if (requestedQuantity > product.StockQuantity)
        {
            TempData["CartError"] = $"Only {product.StockQuantity} item(s) are available.";
            return RedirectToAction("Details", "Catalogue", new { id = product.Id });
        }

        if (existingItem is null)
        {
            cartItems.Add(new CartSessionItem
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = model.Quantity,
                ImageUrl = product.ImageUrl
            });
        }
        else
        {
            existingItem.Quantity = requestedQuantity;
            existingItem.Name = product.Name;
            existingItem.UnitPrice = product.Price;
            existingItem.ImageUrl = product.ImageUrl;
        }

        SetCartItems(cartItems);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CartItemInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["CartError"] = "Quantity must be at least 1.";
            return RedirectToAction(nameof(Index));
        }

        var product = await context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == model.ProductId && product.IsActive);

        if (product is null)
        {
            RemoveCartItem(model.ProductId);
            TempData["CartError"] = "That product is no longer available.";
            return RedirectToAction(nameof(Index));
        }

        if (model.Quantity > product.StockQuantity)
        {
            TempData["CartError"] = $"Only {product.StockQuantity} item(s) are available for {product.Name}.";
            return RedirectToAction(nameof(Index));
        }

        var cartItems = GetCartItems();
        var item = cartItems.SingleOrDefault(item => item.ProductId == model.ProductId);
        if (item is not null)
        {
            item.Quantity = model.Quantity;
            item.Name = product.Name;
            item.UnitPrice = product.Price;
            item.ImageUrl = product.ImageUrl;
            SetCartItems(cartItems);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        RemoveCartItem(productId);
        return RedirectToAction(nameof(Index));
    }

    private List<CartSessionItem> GetCartItems()
    {
        var serializedCart = HttpContext.Session.GetString(CartSessionKey);
        return string.IsNullOrWhiteSpace(serializedCart)
            ? []
            : JsonSerializer.Deserialize<List<CartSessionItem>>(serializedCart) ?? [];
    }

    private void SetCartItems(List<CartSessionItem> items)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
    }

    private void RemoveCartItem(int productId)
    {
        var items = GetCartItems();
        items.RemoveAll(item => item.ProductId == productId);
        SetCartItems(items);
    }
}
