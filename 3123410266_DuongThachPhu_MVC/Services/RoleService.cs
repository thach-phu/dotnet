using MVC.Data;
using MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

public sealed class RoleService(StoreDbContext db, RoleManager<Role> roleManager)
{
    public List<Role> GetRoles() => db.Roles.OrderBy(r => r.Id).ToList();

    public List<string> GetPermissionCodes(int roleId) =>
        db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Join(db.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
            .ToList();

    public async Task<(bool Success, string? Error, Role? Role)> CreateRoleAsync(string name, string? description)
    {
        name = name.Trim();
        if (await roleManager.RoleExistsAsync(name))
        {
            return (false, "Tên vai trò này đã tồn tại.", null);
        }

        var role = new Role { Name = name, Description = description, IsSystemRole = false };
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)), null);
        }

        return (true, null, role);
    }

    public async Task<(bool Success, string? Error)> DeleteRoleAsync(int roleId)
    {
        var role = db.Roles.Find(roleId);
        if (role is null) return (false, "Không tìm thấy vai trò.");

        if (role.IsSystemRole)
        {
            return (false, "Không thể xóa vai trò hệ thống.");
        }

        if (db.UserRoles.Any(ur => ur.RoleId == roleId))
        {
            return (false, "Không thể xóa vai trò đang có người dùng.");
        }

        var rolePermissions = db.RolePermissions.Where(rp => rp.RoleId == roleId);
        db.RolePermissions.RemoveRange(rolePermissions);
        await db.SaveChangesAsync();

        var result = await roleManager.DeleteAsync(role);
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
        var existing = db.RolePermissions.Where(rp => rp.RoleId == roleId);
        db.RolePermissions.RemoveRange(existing);

        var permissionIds = db.Permissions
            .Where(p => permissionCodes.Contains(p.Code))
            .Select(p => p.Id)
            .ToList();

        foreach (var permissionId in permissionIds)
        {
            db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        }

        db.SaveChanges();
    }

    public List<Permission> GetPermissionCatalog() => db.Permissions.OrderBy(p => p.Id).ToList();
}
