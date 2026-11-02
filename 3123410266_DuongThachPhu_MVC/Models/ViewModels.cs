using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public sealed class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public string? Search { get; init; }
    public string? Category { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 8;
    public int TotalPages { get; init; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public sealed class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public sealed class RegisterViewModel
{
    [Required] public string FullName { get; set; } = "";
    [Required] public string Phone { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}

public sealed class CheckoutViewModel
{
    [Required] public string CustomerName { get; set; } = "";
    [Required] public string CustomerPhone { get; set; } = "";
    [Required] public string Address { get; set; } = "";
    public string? Note { get; set; }
}

public sealed class CartLineViewModel(Product product, int quantity)
{
    public Product Product { get; } = product;
    public int Quantity { get; } = quantity;
    public decimal Total => Product.Price * Quantity;
}

public sealed class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Items { get; init; } = [];
    public decimal SubTotal => Items.Sum(item => item.Total);
    public decimal Shipping => SubTotal == 0 || SubTotal >= 500000 ? 0 : 30000;
    public decimal GrandTotal => SubTotal + Shipping;
}

public sealed class ProductEditViewModel
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = "";
    [Required] public string Category { get; set; } = "";
    [Range(0.01, double.MaxValue)] public decimal Price { get; set; }
    public string Description { get; set; } = "";
    public string Emoji { get; set; } = "";
    public bool IsPopular { get; set; }
    public string ImageUrl { get; set; } = "";
    [Range(0, int.MaxValue)] public int Stock { get; set; } = 100;
}

public sealed class RoleEditViewModel
{
    [Required] public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public sealed class RolePermissionsViewModel
{
    public int RoleId { get; set; }
    public string[] PermissionCodes { get; set; } = [];
}

public sealed class NewAccountViewModel
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
    public int RoleId { get; set; }
}

public sealed class CategoryEditViewModel
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
}
