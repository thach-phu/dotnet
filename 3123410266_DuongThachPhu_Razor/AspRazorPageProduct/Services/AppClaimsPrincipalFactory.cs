using System.Security.Claims;
using AspRazorPageProduct.Data;
using AspRazorPageProduct.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AspRazorPageProduct.Services
{
    /// <summary>
    /// Thay cho factory mặc định của Identity: giữ nguyên toàn bộ claim chuẩn
    /// (NameIdentifier, Email, vai trò...) rồi gắn thêm claim "permission" chi
    /// tiết theo từng chức năng (ProductsManage/OrdersManage/UsersManage) lấy từ
    /// bảng RolePermission tự viết — để policy phân quyền hiện có không phải đổi.
    /// </summary>
    public sealed class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        private readonly AppDbContext _db;

        public AppClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            AppDbContext db)
            : base(userManager, roleManager, optionsAccessor)
        {
            _db = db;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            foreach (var nameClaim in identity.FindAll(ClaimTypes.Name).ToList())
            {
                identity.RemoveClaim(nameClaim);
            }
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));

            var roleIds = _db.UserRoles
                .Where(userRole => userRole.UserId == user.Id)
                .Select(userRole => userRole.RoleId)
                .ToList();

            var permissionCodes = _db.RolePermissions
                .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
                .Join(_db.Permissions, rolePermission => rolePermission.PermissionId, permission => permission.Id,
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
}
