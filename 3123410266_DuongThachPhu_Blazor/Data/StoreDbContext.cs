using BlazorWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorWeb.Data;

public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options)
    : IdentityDbContext<User, Role, int>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Phải gọi trước tiên: IdentityDbContext tự cấu hình toàn bộ bảng
        // chuẩn AspNetUsers/AspNetRoles/AspNetUserRoles/AspNetRoleClaims...
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasKey(product => product.Id);

        modelBuilder.Entity<Category>()
            .HasKey(category => category.Id);

        modelBuilder.Entity<Order>()
            .HasKey(order => order.Id);

        modelBuilder.Entity<OrderItem>()
            .HasKey(item => item.Id);

        modelBuilder.Entity<Order>()
            .HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("AspNetUsers");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(permission => permission.Id);
            entity.HasIndex(permission => permission.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(rolePermission => rolePermission.RoleId);
            entity.HasOne<Permission>()
                .WithMany()
                .HasForeignKey(rolePermission => rolePermission.PermissionId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.HasIndex(customer => customer.UserId).IsUnique();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(customer => customer.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Category>().HasData(
            new Category(1, "iPhone 11"),
            new Category(2, "iPhone 12"),
            new Category(3, "iPhone 13"),
            new Category(4, "iPhone 14"),
            new Category(5, "iPhone 15"),
            new Category(6, "iPhone 16"),
            new Category(7, "iPhone 17"),
            new Category(8, "iPhone 18"));

        modelBuilder.Entity<Product>().HasData(
            new Product(1101, "iPhone 11 128GB", "iPhone 11", 8990000, "Camera kép, chip A13 Bionic, màn hình Liquid Retina 6.1 inch.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-11/iphone-11-15_1.jpg") { CategoryId = 1 },
            new Product(1102, "iPhone 11 Pro Max 64GB", "iPhone 11", 10990000, "Màn hình Super Retina XDR, camera Pro và pin dùng cả ngày.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-11/iPhone-11-5.jpg") { CategoryId = 1 },
            new Product(1201, "iPhone 12 64GB", "iPhone 12", 11990000, "Thiết kế cạnh phẳng, màn hình Super Retina XDR và 5G.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-12/iphone-12-1.jpg") { CategoryId = 2 },
            new Product(1202, "iPhone 12 Pro Max 128GB", "iPhone 12", 11590000, "Màn hình lớn, camera Pro với LiDAR và hiệu năng A14 Bionic.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-12/iphone-12-7.jpg") { CategoryId = 2 },
            new Product(1301, "iPhone 13 128GB", "iPhone 13", 14990000, "Chip A15 Bionic, camera kép và thời lượng pin tốt.", "📱", false, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-513339.jpg") { CategoryId = 3 },
            new Product(1302, "iPhone 13 Pro Max 128GB", "iPhone 13", 19990000, "Màn hình ProMotion 120Hz, camera Pro và pin bền bỉ.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-507654.jpg") { CategoryId = 3 },
            new Product(1401, "iPhone 14 128GB", "iPhone 14", 18990000, "Hiệu năng ổn định, camera kép sắc nét, phát hiện va chạm.", "📱", false, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-590067.jpg") { CategoryId = 4 },
            new Product(1402, "iPhone 14 Pro Max 128GB", "iPhone 14", 24990000, "Dynamic Island, camera chính 48MP và màn hình Always-On.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-403207.jpg") { CategoryId = 4 },
            new Product(1501, "iPhone 15 128GB", "iPhone 15", 19990000, "Dynamic Island, camera chính 48MP và cổng USB-C.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-663096.png") { CategoryId = 5 },
            new Product(1502, "iPhone 15 Pro Max 256GB", "iPhone 15", 29990000, "Titan bền nhẹ, chip A17 Pro, camera tele 5x.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-288083.jpg") { CategoryId = 5 },
            new Product(1601, "iPhone 16e 128GB", "iPhone 16", 14290000, "Chip A18, Apple Intelligence và thiết kế gọn gàng.", "📱", false, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-326571.webp") { CategoryId = 6 },
            new Product(1602, "iPhone 16 128GB", "iPhone 16", 22990000, "Chip A18, nút Camera Control và hệ thống camera kép.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-465580.jpg") { CategoryId = 6 },
            new Product(1603, "iPhone 16 Pro Max 256GB", "iPhone 16", 30990000, "Titan, A18 Pro, màn hình ProMotion và camera Fusion 48MP.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/dien-thoai-iphone-16-pro-max-256gb-410296.jpg") { CategoryId = 6 },
            new Product(1701, "iPhone 17e 256GB", "iPhone 17", 20990000, "Thiết kế hiện đại, dung lượng 256GB và hiệu năng mạnh mẽ.", "📱", false, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-141.jpg") { CategoryId = 7 },
            new Product(1702, "iPhone 17 256GB", "iPhone 17", 25490000, "Thế hệ iPhone mới với thiết kế tinh tế và hiệu năng vượt trội.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-7_1.jpg") { CategoryId = 7 },
            new Product(1703, "iPhone 17 Pro 256GB", "iPhone 17", 31990000, "Dòng Pro hiệu năng cao, camera chuyên nghiệp và màn hình mượt.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-32_2.jpg") { CategoryId = 7 },
            new Product(1704, "iPhone 17 Pro Max 256GB", "iPhone 17", 34990000, "Bản Pro Max cao cấp với camera và thời lượng pin hàng đầu.", "📱", true, "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/iphone-17-pro-max-256gb-chinh-hang-301444.jpg") { CategoryId = 7 },
            new Product(1801, "iPhone 18 Pro 256GB", "iPhone 18", 38990000, "Mẫu iPhone Pro thế hệ mới, thông tin và giá tham khảo.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-18/iphone-18-pro/iphone-18-pro.jpg") { CategoryId = 8 },
            new Product(1802, "iPhone 18 Pro Max 256GB", "iPhone 18", 41990000, "Mẫu Pro Max cao cấp, thông tin và giá tham khảo.", "📱", true, "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-18/iphone-18-pro/iphone-18-pro_10_.jpg") { CategoryId = 8 });
    }
}
