using AspRazorPageProduct.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspRazorPageProduct.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Phải gọi trước tiên: IdentityDbContext tự cấu hình toàn bộ bảng
            // chuẩn AspNetUsers/AspNetRoles/AspNetUserRoles/AspNetRoleClaims...
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2);
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

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.HasIndex(c => c.Name).IsUnique();
            });
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Laptop", Description = "Máy tính xách tay" },
                new Category { Id = 2, Name = "Điện thoại", Description = "Điện thoại và máy tính bảng" },
                new Category { Id = 3, Name = "Phụ kiện", Description = "Phụ kiện công nghệ" });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

                entity.HasMany(o => o.Items)
                      .WithOne()
                      .HasForeignKey("OrderId")
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey(c => c.ProductId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);
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

            modelBuilder.Entity<Product>().HasData(SeedProducts());
        }

        private static Product[] SeedProducts() => new[]
        {
            // --- LAPTOP ---
            new Product
            {
                Id = 1,
                Name = "Laptop Dell XPS 13 9320",
                Description = "Laptop mỏng nhẹ cao cấp, Chip Intel Core i7 13th, RAM 16GB, SSD 512GB, Màn hình OLED 3.5K siêu nét.",
                Price = 28500000,
                Category = "Laptop",
                ImageUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=500&auto=format&fit=crop&q=80",
                Stock = 8
            },
            new Product
            {
                Id = 2,
                Name = "Apple MacBook Air M3 2024",
                Description = "Thiết kế siêu mỏng 1.24kg, Chip Apple M3 8-core CPU / 10-core GPU, pin sử dụng 18 tiếng liên tục.",
                Price = 27990000,
                Category = "Laptop",
                ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500&auto=format&fit=crop&q=80",
                Stock = 12
            },
            new Product
            {
                Id = 3,
                Name = "ASUS ROG Strix G16 Gaming",
                Description = "Laptop Gaming đỉnh cao, Intel Core i9-13980HX, Card đồ họa RTX 4070 8GB, Màn hình 240Hz 2K Nebula Display.",
                Price = 45990000,
                Category = "Laptop",
                ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=500&auto=format&fit=crop&q=80",
                Stock = 5
            },
            new Product
            {
                Id = 4,
                Name = "Lenovo ThinkPad X1 Carbon Gen 11",
                Description = "Laptop doanh nhân huyền thoại, chất liệu sợi carbon siêu bền nhẹ 1.12kg, RAM 32GB, màn hình 2.8K OLED.",
                Price = 39900000,
                Category = "Laptop",
                ImageUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500&auto=format&fit=crop&q=80",
                Stock = 6
            },
            new Product
            {
                Id = 5,
                Name = "HP Spectre x360 14 OLED",
                Description = "Laptop xoay gập 360 độ cao cấp, đi kèm bút cảm ứng HP Tilt Pen, màn hình cảm ứng OLED 120Hz.",
                Price = 34500000,
                Category = "Laptop",
                ImageUrl = "https://images.unsplash.com/photo-1544731612-de7f96afe55f?w=500&auto=format&fit=crop&q=80",
                Stock = 7
            },
            new Product
            {
                Id = 6,
                Name = "Acer Swift Go 14 AI",
                Description = "Laptop văn phòng tích hợp NPU AI, AMD Ryzen 7 8840U, RAM 16GB, màn hình IPS 100% sRGB sắc nét.",
                Price = 18990000,
                Category = "Laptop",
                ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=500&auto=format&fit=crop&q=80",
                Stock = 15
            },

            // --- ĐIỆN THOẠI & MÁY TÍNH BẢNG ---
            new Product
            {
                Id = 7,
                Name = "iPhone 15 Pro Max 256GB",
                Description = "Khung vỏ Titanium siêu bền nhẹ, chip A17 Pro 3nm chơi game AAA, Camera Telephoto 5x chuyên nghiệp.",
                Price = 29490000,
                Category = "Điện thoại",
                ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500&auto=format&fit=crop&q=80",
                Stock = 15
            },
            new Product
            {
                Id = 8,
                Name = "Samsung Galaxy S24 Ultra",
                Description = "Quyền năng Galaxy AI thông minh, Camera 200MP zoom quang 10x, Khung Titan, Bút S-Pen tích hợp.",
                Price = 26990000,
                Category = "Điện thoại",
                ImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=500&auto=format&fit=crop&q=80",
                Stock = 10
            },
            new Product
            {
                Id = 9,
                Name = "Xiaomi 14 Ultra Leica 512GB",
                Description = "Hệ thống 4 camera Leica 50MP cảm biến 1 inch, chip Snapdragon 8 Gen 3, sạc siêu nhanh 90W.",
                Price = 24990000,
                Category = "Điện thoại",
                ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500&auto=format&fit=crop&q=80",
                Stock = 9
            },
            new Product
            {
                Id = 10,
                Name = "Google Pixel 8 Pro",
                Description = "Trải nghiệm Android thuần chuẩn Google, chụp ảnh đêm Night Sight bá đạo, hỗ trợ cập nhật phần mềm 7 năm.",
                Price = 19800000,
                Category = "Điện thoại",
                ImageUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=500&auto=format&fit=crop&q=80",
                Stock = 8
            },
            new Product
            {
                Id = 11,
                Name = "OPPO Find N3 Fold 512GB",
                Description = "Smartphone gập cao cấp màn hình không nếp gấp, Hasselblad Camera flagship, chip Snapdragon 8 Gen 2.",
                Price = 38990000,
                Category = "Điện thoại",
                ImageUrl = "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?w=500&auto=format&fit=crop&q=80",
                Stock = 4
            },
            new Product
            {
                Id = 12,
                Name = "iPad Pro 11 inch M4 256GB",
                Description = "Máy tính bảng siêu mỏng 5.3mm, màn hình Tandem OLED Ultra Retina XDR, chip M4 hiệu năng vượt trội.",
                Price = 28490000,
                Category = "Điện thoại",
                ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=500&auto=format&fit=crop&q=80",
                Stock = 11
            },

            // --- PHỤ KIỆN TÍNH NĂNG ---
            new Product
            {
                Id = 13,
                Name = "Bàn phím cơ Keychron K8 Pro",
                Description = "Bàn phím cơ không dây Bluetooth/Type-C, Gateron Pro Switch, LED RGB 16.8 triệu màu, tùy biến QMK/VIA.",
                Price = 2450000,
                Category = "Phụ kiện",
                ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=500&auto=format&fit=crop&q=80",
                Stock = 25
            },
            new Product
            {
                Id = 14,
                Name = "Chuột Logitech MX Master 3S",
                Description = "Con lăn cuộn vô tận MagSpeed, cảm biến 8000 DPI theo dõi trên kính, phím bấm chống ồn Quiet Clicks 90%.",
                Price = 2190000,
                Category = "Phụ kiện",
                ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=500&auto=format&fit=crop&q=80",
                Stock = 30
            },
            new Product
            {
                Id = 15,
                Name = "Đế sạc Anker MagGo 3-in-1 Qi2",
                Description = "Trạm sạc không dây nam chân xếp gọn cho iPhone, Apple Watch và AirPods, công suất sạc nhanh Qi2 15W.",
                Price = 2390000,
                Category = "Phụ kiện",
                ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07?w=500&auto=format&fit=crop&q=80",
                Stock = 20
            },
            new Product
            {
                Id = 16,
                Name = "Hub USB-C HyperDrive 10-in-1",
                Description = "Bộ chuyển đổi đa năng cổng HDMI 4K60Hz, Ethernet 1Gbps, khe thẻ SD/MicroSD, sạc PD 100W vỏ nhôm nguyên khối.",
                Price = 1850000,
                Category = "Phụ kiện",
                ImageUrl = "https://images.unsplash.com/photo-1616440347437-b1c73416efc2?w=500&auto=format&fit=crop&q=80",
                Stock = 18
            },
            new Product
            {
                Id = 17,
                Name = "Bàn phím NuPhy Air75 V2 Low Profile",
                Description = "Bàn phím cơ siêu mỏng thời trang, kết nối 3 chế độ (2.4G 1000Hz, Bluetooth 5.1, Dây), Gateron Low Profile Switch.",
                Price = 3290000,
                Category = "Phụ kiện",
                ImageUrl = "https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?w=500&auto=format&fit=crop&q=80",
                Stock = 14
            },
            new Product
            {
                Id = 18,
                Name = "Chuột Gaming Razer DeathAdder V3 Pro",
                Description = "Trọng lượng siêu nhẹ 63g, Cảm biến Focus Pro 30K DPI, Switch quang học Razer Gen-3 độ bền 90 triệu lần nhấn.",
                Price = 3490000,
                Category = "Phụ kiện",
                ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=500&auto=format&fit=crop&q=80",
                Stock = 16
            },

            // --- ÂM THANH ---
            new Product
            {
                Id = 19,
                Name = "Tai nghe Sony WH-1000XM5",
                Description = "Tai nghe chụp tai chống ồn chủ động ANC hàng đầu thế giới, thời lượng pin 30 giờ, chuẩn âm thanh Hi-Res LDAC.",
                Price = 6990000,
                Category = "Âm thanh",
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&auto=format&fit=crop&q=80",
                Stock = 7
            },
            new Product
            {
                Id = 20,
                Name = "Loa Bluetooth Marshall Stanmore III",
                Description = "Thiết kế cổ điển phong cách Vintage cao cấp, âm trường rộng mở, kết nối Bluetooth 5.2 và cổng RCA/AUX 3.5mm.",
                Price = 8990000,
                Category = "Âm thanh",
                ImageUrl = "https://images.unsplash.com/photo-1545454675-3531b543be5d?w=500&auto=format&fit=crop&q=80",
                Stock = 9
            },
            new Product
            {
                Id = 21,
                Name = "Apple AirPods Pro 2 USB-C",
                Description = "Tai nghe nhét tai chống ồn chủ động gấp 2 lần, chip H2, âm thanh không gian Spatial Audio theo dõi đầu người.",
                Price = 5790000,
                Category = "Âm thanh",
                ImageUrl = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=500&auto=format&fit=crop&q=80",
                Stock = 22
            },
            new Product
            {
                Id = 22,
                Name = "Loa JBL Charge 5 Kháng Nước IP67",
                Description = "Loa di động âm thanh JBL Original Pro Sound sống động, pin 20 tiếng, hỗ trợ sạc ngược cho điện thoại qua cổng USB.",
                Price = 3690000,
                Category = "Âm thanh",
                ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=500&auto=format&fit=crop&q=80",
                Stock = 14
            },
            new Product
            {
                Id = 23,
                Name = "Sennheiser Momentum True Wireless 4",
                Description = "Tai nghe không dây chất âm Audiophile cao cấp, Bluetooth 5.4, aptX Lossless, chống ồn thích ứng Adaptive ANC.",
                Price = 7490000,
                Category = "Âm thanh",
                ImageUrl = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=500&auto=format&fit=crop&q=80",
                Stock = 6
            },
            new Product
            {
                Id = 24,
                Name = "Loa B&O Beosound Explore",
                Description = "Loa dã ngoại vỏ nhôm chống xước đạt chuẩn IP67, quai treo tiện lợi, âm thanh 360 độ âm bass mạnh mẽ.",
                Price = 4990000,
                Category = "Âm thanh",
                ImageUrl = "https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=500&auto=format&fit=crop&q=80",
                Stock = 8
            },

            // --- ĐỒNG HỒ & THIẾT BỊ CÔNG NGHỆ ---
            new Product
            {
                Id = 25,
                Name = "Apple Watch Series 9 GPS 45mm",
                Description = "Đồng hồ thông minh theo dõi sức khỏe chuyên sâu, đo oxy máu SpO2, ECG tim, chip S9 SIP, cử chỉ Double Tap.",
                Price = 9890000,
                Category = "Đồng hồ",
                ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500&auto=format&fit=crop&q=80",
                Stock = 18
            },
            new Product
            {
                Id = 26,
                Name = "Garmin Fenix 7 Pro Solar Edition",
                Description = "Đồng hồ thể thao chuyên nghiệp sạc năng lượng mặt trời, định vị GPS đa băng tần, đèn pin LED tích hợp, pin 22 ngày.",
                Price = 21490000,
                Category = "Đồng hồ",
                ImageUrl = "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?w=500&auto=format&fit=crop&q=80",
                Stock = 7
            },
            new Product
            {
                Id = 27,
                Name = "Màn Hình LG UltraGear 27GP850-B 2K 180Hz",
                Description = "Màn hình đồ họa & Gaming Nano IPS 27 inch QHD, tần số quét 180Hz (OC), thời gian phản hồi 1ms GtG, HDR400.",
                Price = 8490000,
                Category = "Màn hình",
                ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=500&auto=format&fit=crop&q=80",
                Stock = 12
            },
            new Product
            {
                Id = 28,
                Name = "Máy Ảnh Sony Alpha A7 Mark IV (Body)",
                Description = "Máy ảnh Mirrorless Full-frame 33MP, quay phim 4K 60p 10-bit 4:2:2, lấy nét ánh mắt tự động Real-time Eye AF.",
                Price = 54900000,
                Category = "Máy ảnh",
                ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=500&auto=format&fit=crop&q=80",
                Stock = 4
            },
            new Product
            {
                Id = 29,
                Name = "Ghế Công Thái Học Herman Miller Aeron",
                Description = "Ghế văn phòng cao cấp nhập khẩu Mỹ, chất liệu lưới Pellicle thoáng khí, tựa lưng PostureFit SL bảo vệ cột sống tối đa.",
                Price = 36800000,
                Category = "Khác",
                ImageUrl = "https://images.unsplash.com/photo-1580481072645-022f9a6d8310?w=500&auto=format&fit=crop&q=80",
                Stock = 5
            },
            new Product
            {
                Id = 30,
                Name = "Router Wifi 6 TP-Link Archer AX73",
                Description = "Bộ phát Wifi 6 Gigabit 6 dòng dữ liệu tốc độ 5400Mbps, 6 ăng-ten ngoài công suất cao, bảo vệ HomeShield.",
                Price = 2490000,
                Category = "Khác",
                ImageUrl = "https://images.unsplash.com/photo-1544197150-b99a580bb7a8?w=500&auto=format&fit=crop&q=80",
                Stock = 20
            }
        };
    }
}
