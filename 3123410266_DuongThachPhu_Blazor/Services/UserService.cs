using BlazorWeb.Data;
using BlazorWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorWeb.Services;

public sealed class UserService(StoreDbContext db, UserManager<User> userManager)
{
    public async Task<(bool Success, string? Error, User? User)> RegisterAsync(
        string fullName, string phone, string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return (false, "Email này đã được sử dụng.", null);
        }

        var user = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            PhoneNumber = phone.Trim(),
            FullName = fullName.Trim(),
            CreatedAt = DateTime.Now,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return (false, string.Join(" ", createResult.Errors.Select(e => e.Description)), null);
        }

        await userManager.AddToRoleAsync(user, RoleNames.Customer);

        // Khách tự đăng ký cũng là một khách hàng: tạo sẵn bản ghi Khách Hàng,
        // liên kết luôn với tài khoản này để hiện trong trang Quản Lý Khách Hàng.
        db.Customers.Add(new Customer { FullName = user.FullName, Phone = user.PhoneNumber ?? "", UserId = user.Id });
        db.SaveChanges();

        return (true, null, user);
    }

    /// <summary>Admin tạo thẳng 1 tài khoản mới và gán sẵn vai trò (nút "Thêm tài khoản" ở trang quản trị).</summary>
    public async Task<(bool Success, string? Error, User? User)> CreateAccountAsync(
        string fullName, string phone, string email, string password, int roleId)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return (false, "Email này đã được sử dụng.", null);
        }

        var role = db.Roles.Find(roleId);
        if (role is null)
        {
            return (false, "Vai trò không tồn tại.", null);
        }

        var user = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            PhoneNumber = phone.Trim(),
            FullName = fullName.Trim(),
            CreatedAt = DateTime.Now,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return (false, string.Join(" ", createResult.Errors.Select(e => e.Description)), null);
        }

        await userManager.AddToRoleAsync(user, role.Name!);

        if (role.Name == RoleNames.Customer && !string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            db.Customers.Add(new Customer { FullName = user.FullName, Phone = user.PhoneNumber ?? "", UserId = user.Id });
            db.SaveChanges();
        }

        return (true, null, user);
    }

    public async Task<(bool Success, User? User)> ValidateCredentialsAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return (false, null);
        }

        var isValid = await userManager.CheckPasswordAsync(user, password);
        return isValid ? (true, user) : (false, null);
    }

    public User? GetById(int id) => db.Users.FirstOrDefault(u => u.Id == id);

    public List<User> GetAllWithRoles() => db.Users.OrderBy(u => u.Id).ToList();

    public int? GetRoleId(int userId) =>
        db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => (int?)ur.RoleId).FirstOrDefault();

    public string GetRoleName(int userId) =>
        db.UserRoles.Where(ur => ur.UserId == userId)
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .FirstOrDefault() ?? "";

    public async Task<(bool Success, string? Error)> UpdateRoleAsync(int userId, int roleId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "Không tìm thấy người dùng.");

        var newRole = db.Roles.Find(roleId);
        if (newRole is null)
        {
            return (false, "Vai trò không tồn tại.");
        }

        if (await WouldRemoveLastUsersManageHolderAsync(userId, roleId))
        {
            return (false, "Không thể thực hiện: đây là người dùng cuối cùng có quyền quản lý người dùng.");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }
        await userManager.AddToRoleAsync(user, newRole.Name!);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "Không tìm thấy người dùng.");

        if (await WouldRemoveLastUsersManageHolderAsync(userId, newRoleId: -1))
        {
            return (false, "Không thể xóa: đây là người dùng cuối cùng có quyền quản lý người dùng.");
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SetUserLockoutAsync(int userId, bool disabled)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "Không tìm thấy người dùng.");

        if (disabled && await WouldRemoveLastUsersManageHolderAsync(userId, newRoleId: -1))
        {
            return (false, "Không thể vô hiệu hóa: đây là người dùng cuối cùng có quyền quản lý người dùng.");
        }

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, disabled ? DateTimeOffset.UtcNow.AddYears(100) : null);
        return (true, null);
    }

    private async Task<bool> WouldRemoveLastUsersManageHolderAsync(int userId, int newRoleId)
    {
        var usersManagePermissionId = db.Permissions.SingleOrDefault(p => p.Code == Permissions.UsersManage)?.Id;
        if (usersManagePermissionId is null) return false;

        var rolesWithUsersManage = db.RolePermissions
            .Where(rp => rp.PermissionId == usersManagePermissionId)
            .Select(rp => rp.RoleId)
            .ToHashSet();

        var otherHoldersRemain = db.UserRoles
            .Where(ur => ur.UserId != userId)
            .Any(ur => rolesWithUsersManage.Contains(ur.RoleId));

        var newRoleKeepsAccess = rolesWithUsersManage.Contains(newRoleId);

        return !otherHoldersRemain && !newRoleKeepsAccess;
    }
}
