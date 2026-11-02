using Microsoft.AspNetCore.Mvc;
using MVC.Services;

namespace MVC.Controllers;

public sealed class HomeController(ProductCatalog catalog) : Controller
{
    public IActionResult Index() => View(catalog.Products.Where(product => product.IsPopular).ToList());
    public IActionResult Error() => View();
}
