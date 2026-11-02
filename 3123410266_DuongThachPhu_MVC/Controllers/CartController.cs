using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Services;
using System.Security.Claims;

namespace MVC.Controllers;

public sealed class CartController(SessionCart cart, OrderService orders) : Controller
{
    public IActionResult Index() => View(cart.GetCart());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Update(int id, int quantity) { cart.Update(id, quantity); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Remove(int id) { cart.Remove(id); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutViewModel input)
    {
        var state = cart.GetCart();
        if (!ModelState.IsValid || state.Items.Count == 0) return View("Index", state);
        var result = orders.TryCreateOrder(input.CustomerName, input.CustomerPhone, input.Address, input.Note,
            state.Items.Select(item => (item.Product, item.Quantity)).ToList(), state.Shipping,
            userId: int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null);
        if (result.Order is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Không thể tạo đơn hàng.");
            return View("Index", state);
        }

        cart.Clear();
        return View("Success", result.Order);
    }
}
