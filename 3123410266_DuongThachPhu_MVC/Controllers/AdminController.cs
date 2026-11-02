using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Services;

namespace MVC.Controllers;

[Authorize]
public sealed class AdminController(
    ProductCatalog catalog, OrderService orders, UserService users, RoleService roles, CustomerService customers,
    UserManager<User> userManager, SignInManager<User> signInManager, IUserClaimsPrincipalFactory<User> claimsFactory) : Controller
{
    [AllowAnonymous, HttpGet("admin/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && Permissions.HasAnyAdminAccess(User))
        {
            return RedirectToAction(nameof(Index));
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous, HttpPost("admin/login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel input)
    {
        if (!ModelState.IsValid) return View(input);

        var user = await userManager.FindByEmailAsync(input.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, input.Password))
        {
            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác. Vui lòng kiểm tra lại.");
            return View(input);
        }
        if (await userManager.IsLockedOutAsync(user))
        {
            ModelState.AddModelError("", "Tài khoản này đã bị vô hiệu hóa.");
            return View(input);
        }

        var principal = await claimsFactory.CreateAsync(user);
        if (!Permissions.HasAnyAdminAccess(principal))
        {
            ModelState.AddModelError("", "Tài khoản này không có quyền quản trị.");
            return View(input);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return AuthClaimsFactory.IsSafeLocalReturnUrl(input.ReturnUrl)
            ? LocalRedirect(input.ReturnUrl!)
            : RedirectToAction(nameof(Index));
    }

    public IActionResult Index()
    {
        if (!Permissions.HasAnyAdminAccess(User)) return Forbid();
        ViewBag.Orders = orders.GetOrders();
        ViewBag.Dashboard = orders.GetDashboardSummary();
        var allUsers = users.GetAllWithRoles();
        ViewBag.Users = allUsers;
        ViewBag.UserRoleIds = allUsers.ToDictionary(u => u.Id, u => users.GetRoleId(u.Id) ?? 0);
        ViewBag.Roles = roles.GetRoles();
        ViewBag.Permissions = roles.GetPermissionCatalog();
        ViewBag.RolePermissions = roles.GetRoles().ToDictionary(role => role.Id, role => roles.GetPermissionCodes(role.Id));
        ViewBag.Categories = catalog.Categories;
        ViewBag.Customers = customers.GetAll();
        ViewBag.UnlinkedUsers = customers.GetUnlinkedUsers();
        return View(catalog.Products);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.CustomersManage)]
    public IActionResult CreateCustomer(string fullName, string phone)
    {
        var result = customers.Create(fullName, phone);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.CustomersManage)]
    public IActionResult MapCustomer(int customerId, int userId)
    {
        var result = customers.MapToUser(customerId, userId);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.CustomersManage)]
    public IActionResult UpdateCustomer(int customerId, string fullName, string phone)
    {
        var result = customers.Update(customerId, fullName, phone);
        if (result.Success) TempData["AdminSuccess"] = "Đã cập nhật khách hàng.";
        else TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.CustomersManage)]
    public IActionResult DeleteCustomer(int customerId)
    {
        var result = customers.Delete(customerId);
        if (result.Success) TempData["AdminSuccess"] = "Đã xóa khách hàng.";
        else TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = Permissions.ProductsManage)]
    public IActionResult EditProduct(int? id)
    {
        var product = id.HasValue ? catalog.GetById(id.Value) : null;
        if (id.HasValue && product is null) return NotFound();
        ViewBag.Categories = catalog.Categories;
        return View(product is null ? new ProductEditViewModel() : ToEditModel(product));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.ProductsManage)]
    public IActionResult SaveProduct(ProductEditViewModel input)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = catalog.Categories;
            return View("EditProduct", input);
        }

        var product = new Product(input.Id, input.Name.Trim(), input.Category, input.Price,
            input.Description.Trim(), input.Emoji.Trim(), input.IsPopular, input.ImageUrl.Trim())
        {
            Stock = input.Stock
        };
        if (input.Id == 0) catalog.Add(product);
        else if (!catalog.Update(product)) return NotFound();
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.ProductsManage)]
    public IActionResult DeleteProduct(int id) { catalog.Remove(id); return RedirectToAction(nameof(Index)); }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.CategoriesManage)]
    public IActionResult CreateCategory(CategoryEditViewModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["AdminError"] = "Tên danh mục không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }
        var result = catalog.AddCategoryForAdmin(input.Name);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.CategoriesManage)]
    public IActionResult UpdateCategory(CategoryEditViewModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["AdminError"] = "Tên danh mục không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }
        var result = catalog.UpdateCategory(input.Id, input.Name);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.CategoriesManage)]
    public IActionResult DeleteCategory(int id)
    {
        var result = catalog.RemoveCategory(id);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }
    [HttpGet, Authorize(Policy = Permissions.OrdersManage)]
    public IActionResult OrderDetails(int id)
    {
        var order = orders.GetOrders().FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        ViewBag.ValidNextStatuses = orders.GetValidNextStatuses(order.Status);
        return View(order);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.OrdersManage)]
    public IActionResult UpdateOrder(int id, string status)
    {
        var result = orders.UpdateOrderStatus(id, status);
        if (!result.Success)
        {
            TempData["AdminError"] = result.Error;
            if (!orders.GetOrders().Any(o => o.Id == id))
            {
                // Đơn hàng không còn tồn tại — quay về danh sách thay vì điều hướng
                // tới trang chi tiết (sẽ trả 404 và người dùng không thấy lỗi này).
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(OrderDetails), new { id });
        }
        TempData["AdminSuccess"] = "Đã cập nhật trạng thái đơn hàng.";
        return RedirectToAction(nameof(OrderDetails), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.OrdersManage)]
    public IActionResult CancelOrder(int id)
    {
        var result = orders.UpdateOrderStatus(id, "Đã hủy");
        if (result.Success) TempData["AdminSuccess"] = "Đã hủy đơn hàng.";
        else TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> UpdateUserRole(int userId, int roleId)
    {
        var result = await users.UpdateRoleAsync(userId, roleId);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> SetUserLockout(int userId, bool disabled)
    {
        var result = await users.SetUserLockoutAsync(userId, disabled);
        if (result.Success) TempData["AdminSuccess"] = disabled ? "Đã tắt tài khoản." : "Đã bật lại tài khoản.";
        else TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var result = await users.DeleteUserAsync(userId);
        if (result.Success) TempData["AdminSuccess"] = "Đã xóa tài khoản.";
        else TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> CreateAccount(NewAccountViewModel input)
    {
        if (!ModelState.IsValid) { TempData["AdminError"] = "Thông tin tài khoản không hợp lệ."; return RedirectToAction(nameof(Index)); }
        var result = await users.CreateAccountAsync(input.FullName, input.Phone, input.Email, input.Password, input.RoleId);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> CreateRole(RoleEditViewModel input)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        var result = await roles.CreateRoleAsync(input.Name.Trim(), input.Description?.Trim());
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> DeleteRole(int roleId)
    {
        var result = await roles.DeleteRoleAsync(roleId);
        if (!result.Success) TempData["AdminError"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = Permissions.UsersManage)]
    public IActionResult SaveRolePermissions(RolePermissionsViewModel input)
    {
        roles.SetRolePermissions(input.RoleId, input.PermissionCodes);
        return RedirectToAction(nameof(Index));
    }

    private static ProductEditViewModel ToEditModel(Product product) => new()
    {
        Id = product.Id, Name = product.Name, Category = product.Category, Price = product.Price,
        Description = product.Description, Emoji = product.Emoji, IsPopular = product.IsPopular,
        ImageUrl = product.ImageUrl,
        Stock = product.Stock
    };
}
