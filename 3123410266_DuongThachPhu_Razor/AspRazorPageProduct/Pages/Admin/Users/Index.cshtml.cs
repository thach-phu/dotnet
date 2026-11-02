using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public IndexModel(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        public List<AspRazorPageProduct.Models.User> Users { get; set; } = [];
        public List<Role> Roles { get; set; } = [];
        public List<Permission> PermissionCatalog { get; set; } = [];
        public Dictionary<int, List<string>> RolePermissions { get; set; } = [];
        public Dictionary<int, int> UserRoleIds { get; set; } = [];

        public void OnGet() => LoadData();

        public async Task<IActionResult> OnPostUpdateUserRole(int userId, int roleId)
        {
            var result = await _userService.UpdateRoleAsync(userId, roleId);
            if (!result.Success) TempData["AdminError"] = result.Error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAccount(string fullName, string phone, string email, string password, int roleId)
        {
            var result = await _userService.CreateAccountAsync(fullName, phone, email, password, roleId);
            if (!result.Success) TempData["AdminError"] = result.Error;
            else TempData["AdminSuccess"] = $"Đã tạo tài khoản: {email}";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetUserLockout(int userId, bool disabled)
        {
            var result = await _userService.SetUserLockoutAsync(userId, disabled);
            if (result.Success) TempData["AdminSuccess"] = disabled ? "Đã tắt tài khoản." : "Đã bật lại tài khoản.";
            else TempData["AdminError"] = result.Error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUser(int userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
            if (result.Success) TempData["AdminSuccess"] = "Đã xóa tài khoản.";
            else TempData["AdminError"] = result.Error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateRole(string name, string? description)
        {
            var result = await _roleService.CreateRoleAsync(name, description);
            if (!result.Success) TempData["AdminError"] = result.Error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteRole(int roleId)
        {
            var result = await _roleService.DeleteRoleAsync(roleId);
            if (!result.Success) TempData["AdminError"] = result.Error;
            return RedirectToPage();
        }

        public IActionResult OnPostSaveRolePermissions(int roleId, List<string>? permissionCodes)
        {
            _roleService.SetRolePermissions(roleId, permissionCodes ?? []);
            TempData["AdminSuccess"] = "Đã lưu danh sách quyền cho vai trò.";
            return RedirectToPage();
        }

        private void LoadData()
        {
            Users = _userService.GetAllWithRoles();
            Roles = _roleService.GetRoles();
            PermissionCatalog = _roleService.GetPermissionCatalog();
            RolePermissions = Roles.ToDictionary(role => role.Id, role => _roleService.GetPermissionCodes(role.Id));
            UserRoleIds = Users.ToDictionary(user => user.Id, user => _userService.GetRoleId(user.Id) ?? 0);
        }
    }
}
