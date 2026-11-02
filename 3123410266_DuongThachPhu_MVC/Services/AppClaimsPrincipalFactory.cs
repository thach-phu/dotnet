using System.Security.Claims;
using MVC.Data;
using MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MVC.Services;

/// <summary>
/// Thay cho factory mặc định của Identity: giữ nguyên toàn bộ claim chuẩn
/// (NameIdentifier, Email, vai trò...) rồi gắn thêm claim "permission" chi
/// tiết theo từng chức năng (ProductsManage/OrdersManage/UsersManage) lấy từ
/// bảng RolePermission tự viết — để policy phân quyền hiện có không phải đổi.
/// </summary>
public sealed class AppClaimsPrincipalFactory(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IOptions<IdentityOptions> optionsAccessor,
    StoreDbContext db)
    : UserClaimsPrincipalFactory<User, Role>(userManager, roleManager, optionsAccessor)
{
    public override async Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        foreach (var nameClaim in identity.FindAll(ClaimTypes.Name).ToList())
        {
            identity.RemoveClaim(nameClaim);
        }
        identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));

        var roleIds = db.UserRoles
            .Where(userRole => userRole.UserId == user.Id)
            .Select(userRole => userRole.RoleId)
            .ToList();

        var permissionCodes = db.RolePermissions
            .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
            .Join(db.Permissions, rolePermission => rolePermission.PermissionId, permission => permission.Id,
                (_, permission) => permission.Code)
            .Distinct()
            .ToList();

        foreach (var code in permissionCodes)
        {
            identity.AddClaim(new Claim(Permissions.ClaimType, code));
        }

        return principal;
    }
}
