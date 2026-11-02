using MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace MVC.Data;

public static class DataSeeder
{
    public static async Task SeedAccountsAsync(StoreDbContext db, UserManager<User> userManager, RoleManager<Role> roleManager)
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

        // Only grant every permission at the moment the Admin role is first
        // created. On later restarts, an admin may have deliberately revoked
        // a permission from this role via the Users & Roles tab — seeding
        // must not silently undo that edit. But a permission code that is
        // brand new to the codebase (e.g. categories.manage added later)
        // still needs to reach the Admin role, otherwise no one can grant it
        // through the UI since the UsersManage screen itself is gated.
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
            var customerUser = new User
            {
                UserName = "user@gmail.com",
                Email = "user@gmail.com",
                PhoneNumber = "0900000001",
                FullName = "Người dùng",
                CreatedAt = DateTime.Now,
            };
            var result = await userManager.CreateAsync(customerUser, "admin123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, RoleNames.Customer);
                db.Customers.Add(new Customer { FullName = customerUser.FullName, Phone = customerUser.PhoneNumber ?? "", UserId = customerUser.Id });
                await db.SaveChangesAsync();
            }
        }

        SeedProducts(db);
    }

    private static void SeedProducts(StoreDbContext db)
    {
        if (db.Products.Any())
        {
            var seededImages = CreateProducts().ToDictionary(product => product.Id, product => product.ImageUrl);
            var missingImages = db.Products
                .Where(product => string.IsNullOrWhiteSpace(product.ImageUrl))
                .ToList();
            foreach (var product in missingImages)
            {
                if (seededImages.TryGetValue(product.Id, out var imageUrl))
                {
                    db.Entry(product).Property(existing => existing.ImageUrl).CurrentValue = imageUrl;
                }
            }
            if (missingImages.Any(product => !string.IsNullOrWhiteSpace(product.ImageUrl)))
            {
                db.SaveChanges();
            }
            return;
        }

        var products = CreateProducts();
        var categories = products
            .Select(product => product.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select((name, index) => new Category(index + 1, name))
            .ToList();
        var categoryIds = categories.ToDictionary(category => category.Name, category => category.Id);

        db.Categories.AddRange(categories);
        db.Products.AddRange(products.Select(product =>
            product with { CategoryId = categoryIds[product.Category] }));
        db.SaveChanges();
    }

    private static List<Product> CreateProducts()
    {
        var products = new List<Product>
        {
        new(1101, "iPhone 11 128GB", "iPhone 11", 8990000, "Camera kép, chip A13 Bionic, màn hình Liquid Retina 6.1 inch.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-11/iphone-11-15_1.jpg"),
        new(1102, "iPhone 11 Pro Max 64GB", "iPhone 11", 10990000, "Màn hình Super Retina XDR, camera Pro và pin dùng cả ngày.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-11/iPhone-11-5.jpg"),
        new(1201, "iPhone 12 64GB", "iPhone 12", 11990000, "Thiết kế cạnh phẳng, màn hình Super Retina XDR và 5G.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-12/iphone-12-1.jpg"),
        new(1202, "iPhone 12 Pro Max 128GB", "iPhone 12", 11590000, "Màn hình lớn, camera Pro với LiDAR và hiệu năng A14 Bionic.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-12/iphone-12-7.jpg"),
        new(1301, "iPhone 13 128GB", "iPhone 13", 14990000, "Chip A15 Bionic, camera kép và thời lượng pin tốt.", "📱", false, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-513339.jpg"),
        new(1302, "iPhone 13 Pro Max 128GB", "iPhone 13", 19990000, "Màn hình ProMotion 120Hz, camera Pro và pin bền bỉ.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-507654.jpg"),
        new(1401, "iPhone 14 128GB", "iPhone 14", 18990000, "Hiệu năng ổn định, camera kép sắc nét, phát hiện va chạm.", "📱", false, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-590067.jpg"),
        new(1402, "iPhone 14 Pro Max 128GB", "iPhone 14", 24990000, "Dynamic Island, camera chính 48MP và màn hình Always-On.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-403207.jpg"),
        new(1501, "iPhone 15 128GB", "iPhone 15", 19990000, "Dynamic Island, camera chính 48MP và cổng USB-C.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-663096.png"),
        new(1502, "iPhone 15 Pro Max 256GB", "iPhone 15", 29990000, "Titan bền nhẹ, chip A17 Pro, camera tele 5x.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-288083.jpg"),
        new(1601, "iPhone 16e 128GB", "iPhone 16", 14290000, "Chip A18, Apple Intelligence và thiết kế gọn gàng.", "📱", false, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-326571.webp"),
        new(1602, "iPhone 16 128GB", "iPhone 16", 22990000, "Chip A18, nút Camera Control và hệ thống camera kép.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-465580.jpg"),
        new(1603, "iPhone 16 Pro Max 256GB", "iPhone 16", 30990000, "Titan, A18 Pro, màn hình ProMotion và camera Fusion 48MP.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/dien-thoai-iphone-16-pro-max-256gb-410296.jpg"),
        new(1701, "iPhone 17e 256GB", "iPhone 17", 20990000, "Thiết kế hiện đại, dung lượng 256GB và hiệu năng mạnh mẽ.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-141.jpg"),
        new(1702, "iPhone 17 256GB", "iPhone 17", 25490000, "Thế hệ iPhone mới với thiết kế tinh tế và hiệu năng vượt trội.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-7_1.jpg"),
        new(1703, "iPhone 17 Pro 256GB", "iPhone 17", 31990000, "Dòng Pro hiệu năng cao, camera chuyên nghiệp và màn hình mượt.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-32_2.jpg"),
        new(1704, "iPhone 17 Pro Max 256GB", "iPhone 17", 34990000, "Bản Pro Max cao cấp với camera và thời lượng pin hàng đầu.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/iphone-17-pro-max-256gb-chinh-hang-301444.jpg"),
        new(1801, "iPhone 18 Pro 256GB", "iPhone 18", 38990000, "Mẫu iPhone Pro thế hệ mới, thông tin và giá tham khảo.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-18/iphone-18-pro/iphone-18-pro.jpg"),
        new(1802, "iPhone 18 Pro Max 256GB", "iPhone 18", 41990000, "Mẫu Pro Max cao cấp, thông tin và giá tham khảo.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-18/iphone-18-pro/iphone-18-pro_10_.jpg")
        };

        var categoryIds = products
            .Select(product => product.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select((name, index) => new { name, id = index + 1 })
            .ToDictionary(category => category.name, category => category.id, StringComparer.OrdinalIgnoreCase);

        return products
            .Select(product => product with { CategoryId = categoryIds[product.Category] })
            .ToList();
    }
}

