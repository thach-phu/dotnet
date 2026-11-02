using MVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MVC.Data;

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
        modelBuilder.Entity<Product>()
            .Property(product => product.Stock)
            .HasDefaultValue(100);

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
            .HasOne(order => order.User)
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.SetNull);

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
    }
}
