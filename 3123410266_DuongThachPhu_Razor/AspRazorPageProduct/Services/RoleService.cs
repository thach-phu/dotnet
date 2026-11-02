using AspRazorPageProduct.Data;
using AspRazorPageProduct.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspRazorPageProduct.Services
{
    public sealed class RoleService : IRoleService
    {
        private readonly AppDbContext _db;
        private readonly RoleManager<Role> _roleManager;

        public RoleService(AppDbContext db, RoleManager<Role> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
        }

        public List<Role> GetRoles() => _db.Roles.OrderBy(r => r.Id).ToList();

        public List<string> GetPermissionCodes(int roleId) =>
            _db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Join(_db.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
                .ToList();

        public List<Permission> GetPermissionCatalog() => _db.Permissions.OrderBy(p => p.Id).ToList();

        public async Task<(bool Success, string? Error, Role? Role)> CreateRoleAsync(string name, string? description)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Tên vai trò không được để trống.", null);
            }

            if (await _roleManager.RoleExistsAsync(name))
            {
                return (false, "Tên vai trò này đã tồn tại.", null);
            }

            var role = new Role { Name = name, Description = description?.Trim(), IsSystemRole = false };
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)), null);
            }

            return (true, null, role);
        }

        public async Task<(bool Success, string? Error)> DeleteRoleAsync(int roleId)
        {
            var role = _db.Roles.Find(roleId);
            if (role is null) return (false, "Không tìm thấy vai trò.");

            if (role.IsSystemRole)
            {
                return (false, "Không thể xóa vai trò hệ thống.");
            }

            if (_db.UserRoles.Any(ur => ur.RoleId == roleId))
            {
                return (false, "Không thể xóa vai trò đang có người dùng.");
            }

            var rolePermissions = _db.RolePermissions.Where(rp => rp.RoleId == roleId);
            _db.RolePermissions.RemoveRange(rolePermissions);
            await _db.SaveChangesAsync();

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                // Một user khác vừa được gán vào role này giữa lúc kiểm tra và lúc xóa
                // (2 admin cùng sửa) — ràng buộc của Identity đã chặn xóa thay vì để mồ côi dữ liệu.
                return (false, "Không thể xóa vai trò đang có người dùng.");
            }

            return (true, null);
        }

        public void SetRolePermissions(int roleId, IReadOnlyCollection<string> permissionCodes)
        {
            var existing = _db.RolePermissions.Where(rp => rp.RoleId == roleId);
            _db.RolePermissions.RemoveRange(existing);

            var permissionIds = _db.Permissions
                .Where(p => permissionCodes.Contains(p.Code))
                .Select(p => p.Id)
                .ToList();

            foreach (var permissionId in permissionIds)
            {
                _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
            }

            _db.SaveChanges();
        }
    }
}
