using AspRazorPageProduct.Data;
using AspRazorPageProduct.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspRazorPageProduct.Services
{
    public sealed class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;

        public UserService(AppDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<User> GetAllWithRoles() => _db.Users.OrderBy(u => u.Id).ToList();

        public User? GetById(int id) => _db.Users.FirstOrDefault(u => u.Id == id);

        public int? GetRoleId(int userId) =>
            _db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => (int?)ur.RoleId).FirstOrDefault();

        public async Task<(bool Success, string? Error)> UpdateRoleAsync(int userId, int roleId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return (false, "Không tìm thấy người dùng.");

            var newRole = _db.Roles.Find(roleId);
            if (newRole is null)
            {
                return (false, "Vai trò không tồn tại.");
            }

            if (await WouldRemoveLastUsersManageHolderAsync(userId, roleId))
            {
                return (false, "Không thể thực hiện: đây là người dùng cuối cùng có quyền quản lý người dùng.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, newRole.Name!);

            return (true, null);
        }

        /// <summary>Admin tạo thẳng 1 tài khoản mới và gán sẵn vai trò (nút "Thêm tài khoản" ở trang quản trị).</summary>
        public async Task<(bool Success, string? Error, User? User)> CreateAccountAsync(
            string? fullName, string? phone, string email, string password, int roleId)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            if (await _userManager.FindByEmailAsync(normalizedEmail) is not null)
            {
                return (false, "Email này đã được sử dụng.", null);
            }

            var role = _db.Roles.Find(roleId);
            if (role is null)
            {
                return (false, "Vai trò không tồn tại.", null);
            }

            var user = new User
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                PhoneNumber = phone?.Trim() ?? "",
                FullName = fullName?.Trim() ?? "",
                CreatedAt = DateTime.Now,
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return (false, string.Join(" ", createResult.Errors.Select(e => e.Description)), null);
            }

            await _userManager.AddToRoleAsync(user, role.Name!);

            if (role.Name == RoleNames.Customer && !string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                _db.Customers.Add(new Customer { FullName = user.FullName, Phone = user.PhoneNumber ?? "", UserId = user.Id });
                _db.SaveChanges();
            }

            return (true, null, user);
        }

        public async Task<(bool Success, string? Error)> DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return (false, "Không tìm thấy người dùng.");

            if (await WouldRemoveLastUsersManageHolderAsync(userId, newRoleId: -1))
            {
                return (false, "Không thể xóa: đây là người dùng cuối cùng có quyền quản lý người dùng.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
            }
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> SetUserLockoutAsync(int userId, bool disabled)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return (false, "Không tìm thấy người dùng.");

            if (disabled && await WouldRemoveLastUsersManageHolderAsync(userId, newRoleId: -1))
            {
                return (false, "Không thể vô hiệu hóa: đây là người dùng cuối cùng có quyền quản lý người dùng.");
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, disabled ? DateTimeOffset.UtcNow.AddYears(100) : null);
            return (true, null);
        }

        private async Task<bool> WouldRemoveLastUsersManageHolderAsync(int userId, int newRoleId)
        {
            var usersManagePermissionId = _db.Permissions.SingleOrDefault(p => p.Code == Permissions.UsersManage)?.Id;
            if (usersManagePermissionId is null) return false;

            var rolesWithUsersManage = _db.RolePermissions
                .Where(rp => rp.PermissionId == usersManagePermissionId)
                .Select(rp => rp.RoleId)
                .ToHashSet();

            var otherHoldersRemain = _db.UserRoles
                .Where(ur => ur.UserId != userId)
                .Any(ur => rolesWithUsersManage.Contains(ur.RoleId));

            var newRoleKeepsAccess = rolesWithUsersManage.Contains(newRoleId);

            return !otherHoldersRemain && !newRoleKeepsAccess;
        }
    }
}
