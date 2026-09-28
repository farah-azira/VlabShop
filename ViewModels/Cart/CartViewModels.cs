using System.ComponentModel.DataAnnotations;

namespace VlabShop.Web.ViewModels.Cart;

public class CartItemInputModel
{
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;
}

public class CartSessionItem
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public string? ImageUrl { get; set; }
}

public class CartLineViewModel
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public int AvailableStock { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Subtotal => UnitPrice * Quantity;
}

public class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Items { get; set; } = [];

    public decimal Total => Items.Sum(item => item.Subtotal);
}
