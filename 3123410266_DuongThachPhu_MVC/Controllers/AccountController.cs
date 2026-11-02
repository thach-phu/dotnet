using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Services;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers;

public sealed class AccountController(
    UserService users, OrderService orders,
    UserManager<User> userManager, SignInManager<User> signInManager,
    IUserClaimsPrincipalFactory<User> claimsFactory) : Controller
{
    [Authorize]
    [HttpGet]
    public IActionResult Orders() =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId)
            ? View(orders.GetOrdersForUser(userId))
            : Forbid();

    [Authorize]
    [HttpGet]
    public IActionResult OrderDetails(int orderId)
    {
        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Forbid();

        var order = orders.GetOrderForUser(orderId, userId);
        return order is null ? NotFound() : View(order);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public IActionResult CancelOrder(int orderId)
    {
        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Forbid();

        var result = orders.CancelOrderByCustomer(orderId, userId);
        if (!result.Success)
        {
            TempData["CancelError"] = result.Error;
            TempData["CancelledOrderId"] = orderId;
        }
        return RedirectToAction(nameof(Orders));
    }
    [HttpGet] public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel input)
    {
        if (!ModelState.IsValid) return View(input);
        var user = await userManager.FindByEmailAsync(input.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, input.Password))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(input);
        }
        if (await userManager.IsLockedOutAsync(user))
        {
            ModelState.AddModelError("", "Tài khoản này đã bị vô hiệu hóa.");
            return View(input);
        }
        var principal = await claimsFactory.CreateAsync(user);
        if (Permissions.HasAnyAdminAccess(principal))
        {
            ModelState.AddModelError("", "Tài khoản quản trị vui lòng đăng nhập tại trang quản trị (/admin/login).");
            return View(input);
        }

        await signInManager.SignInAsync(user, isPersistent: false);

        if (AuthClaimsFactory.IsSafeLocalReturnUrl(input.ReturnUrl))
        {
            return LocalRedirect(input.ReturnUrl!);
        }

        return RedirectToAction("Index", "Home");
    }
    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel input)
    {
        if (!ModelState.IsValid) return View(input);
        var result = await users.RegisterAsync(input.FullName, input.Phone, input.Email, input.Password);
        if (!result.Success) { ModelState.AddModelError("", result.Error!); return View(input); }
        await signInManager.SignInAsync(result.User!, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
