using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Services;

namespace MVC.Controllers;

public sealed class ProductsController(ProductCatalog catalog, SessionCart cart) : Controller
{
    public IActionResult Index(string? search, string? category, int page = 1)
    {
        const int pageSize = 8;
        page = Math.Max(1, page);
        var products = catalog.Products.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            products = products.Where(product => product.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(category))
            products = products.Where(product => product.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        var filtered = products.ToList();
        var totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)pageSize));
        page = Math.Min(page, totalPages);
        ViewBag.Categories = catalog.Categories;
        return View(new ProductListViewModel
        {
            Products = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Search = search, Category = category, Page = page, PageSize = pageSize, TotalPages = totalPages
        });
    }
    public IActionResult Details(int id) => catalog.GetById(id) is { } product ? View(product) : NotFound();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AddToCart(int id, int quantity = 1, string? returnUrl = null)
    {
        if (catalog.GetById(id) is null) return NotFound();
        cart.Add(id, quantity);
        return LocalRedirect(AuthClaimsFactory.IsSafeLocalReturnUrl(returnUrl) ? returnUrl! : Url.Action(nameof(Index))!);
    }
}
