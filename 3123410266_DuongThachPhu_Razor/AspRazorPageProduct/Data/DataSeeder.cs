using AspRazorPageProduct.Models;
using Microsoft.AspNetCore.Identity;

namespace AspRazorPageProduct.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAccountsAsync(AppDbContext db, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            var addedPermissionCodes = new List<string>();
            foreach (var (code, description) in Permissions.Catalog)
            {
                if (!db.Permissions.Any(p => p.Code == code))
                {
                    db.Permissions.Add(new Permission { Code = code, Description = description });
                    addedPermissionCodes.Add(code);
                }
            }
            await db.SaveChangesAsync();

            var adminRoleIsNew = !await roleManager.RoleExistsAsync(RoleNames.Admin);
            if (adminRoleIsNew)
            {
                await roleManager.CreateAsync(new Role { Name = RoleNames.Admin, Description = "Toàn quyền quản trị hệ thống", IsSystemRole = true });
            }
            if (!await roleManager.RoleExistsAsync(RoleNames.Customer))
            {
                await roleManager.CreateAsync(new Role { Name = RoleNames.Customer, Description = "Khách hàng mua sắm", IsSystemRole = true });
            }

            var adminRole = await roleManager.FindByNameAsync(RoleNames.Admin);
            var customerRole = await roleManager.FindByNameAsync(RoleNames.Customer);

            // Chỉ cấp toàn bộ quyền cho Admin vào đúng lúc Role này được tạo lần đầu.
            // Ở các lần khởi động sau, Admin có thể đã chủ động thu hồi 1 quyền nào đó
            // qua trang Admin/Users — seed không được âm thầm ghi đè lại thao tác đó.
            if (adminRoleIsNew)
            {
                foreach (var permissionId in db.Permissions.Select(p => p.Id).ToList())
                {
                    db.RolePermissions.Add(new RolePermission { RoleId = adminRole!.Id, PermissionId = permissionId });
                }
                await db.SaveChangesAsync();
            }
            else if (addedPermissionCodes.Count > 0)
            {
                var existingCodes = db.RolePermissions
                    .Where(item => item.RoleId == adminRole!.Id)
                    .Join(db.Permissions, item => item.PermissionId, permission => permission.Id,
                        (_, permission) => permission.Code)
                    .ToHashSet();
                foreach (var code in addedPermissionCodes.Where(code => !existingCodes.Contains(code)))
                {
                    var permission = db.Permissions.Single(item => item.Code == code);
                    db.RolePermissions.Add(new RolePermission { RoleId = adminRole!.Id, PermissionId = permission.Id });
                }
                await db.SaveChangesAsync();
            }

            if (await userManager.FindByEmailAsync("admin@gmail.com") is null)
            {
                var admin = new User
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    PhoneNumber = "0900000000",
                    FullName = "Quản trị viên",
                    CreatedAt = DateTime.Now,
                };
                var result = await userManager.CreateAsync(admin, "admin123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, RoleNames.Admin);
                }
            }

            if (await userManager.FindByEmailAsync("user@gmail.com") is null)
            {
                var customer = new User
                {
                    UserName = "user@gmail.com",
                    Email = "user@gmail.com",
                    PhoneNumber = "0900000001",
                    FullName = "Người dùng",
                    CreatedAt = DateTime.Now,
                };
                var result = await userManager.CreateAsync(customer, "admin123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customer, RoleNames.Customer);
                    db.Customers.Add(new Customer { FullName = customer.FullName, Phone = customer.PhoneNumber ?? "", UserId = customer.Id });
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
