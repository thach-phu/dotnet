using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AspRazorPageProduct.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPhone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerEmail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShippingAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CartItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItem_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItem_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1, "Laptop", "Laptop mỏng nhẹ cao cấp, Chip Intel Core i7 13th, RAM 16GB, SSD 512GB, Màn hình OLED 3.5K siêu nét.", "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=500&auto=format&fit=crop&q=80", "Laptop Dell XPS 13 9320", 28500000m, 8 },
                    { 2, "Laptop", "Thiết kế siêu mỏng 1.24kg, Chip Apple M3 8-core CPU / 10-core GPU, pin sử dụng 18 tiếng liên tục.", "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500&auto=format&fit=crop&q=80", "Apple MacBook Air M3 2024", 27990000m, 12 },
                    { 3, "Laptop", "Laptop Gaming đỉnh cao, Intel Core i9-13980HX, Card đồ họa RTX 4070 8GB, Màn hình 240Hz 2K Nebula Display.", "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=500&auto=format&fit=crop&q=80", "ASUS ROG Strix G16 Gaming", 45990000m, 5 },
                    { 4, "Laptop", "Laptop doanh nhân huyền thoại, chất liệu sợi carbon siêu bền nhẹ 1.12kg, RAM 32GB, màn hình 2.8K OLED.", "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500&auto=format&fit=crop&q=80", "Lenovo ThinkPad X1 Carbon Gen 11", 39900000m, 6 },
                    { 5, "Laptop", "Laptop xoay gập 360 độ cao cấp, đi kèm bút cảm ứng HP Tilt Pen, màn hình cảm ứng OLED 120Hz.", "https://images.unsplash.com/photo-1544731612-de7f96afe55f?w=500&auto=format&fit=crop&q=80", "HP Spectre x360 14 OLED", 34500000m, 7 },
                    { 6, "Laptop", "Laptop văn phòng tích hợp NPU AI, AMD Ryzen 7 8840U, RAM 16GB, màn hình IPS 100% sRGB sắc nét.", "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=500&auto=format&fit=crop&q=80", "Acer Swift Go 14 AI", 18990000m, 15 },
                    { 7, "Điện thoại", "Khung vỏ Titanium siêu bền nhẹ, chip A17 Pro 3nm chơi game AAA, Camera Telephoto 5x chuyên nghiệp.", "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500&auto=format&fit=crop&q=80", "iPhone 15 Pro Max 256GB", 29490000m, 15 },
                    { 8, "Điện thoại", "Quyền năng Galaxy AI thông minh, Camera 200MP zoom quang 10x, Khung Titan, Bút S-Pen tích hợp.", "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=500&auto=format&fit=crop&q=80", "Samsung Galaxy S24 Ultra", 26990000m, 10 },
                    { 9, "Điện thoại", "Hệ thống 4 camera Leica 50MP cảm biến 1 inch, chip Snapdragon 8 Gen 3, sạc siêu nhanh 90W.", "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500&auto=format&fit=crop&q=80", "Xiaomi 14 Ultra Leica 512GB", 24990000m, 9 },
                    { 10, "Điện thoại", "Trải nghiệm Android thuần chuẩn Google, chụp ảnh đêm Night Sight bá đạo, hỗ trợ cập nhật phần mềm 7 năm.", "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=500&auto=format&fit=crop&q=80", "Google Pixel 8 Pro", 19800000m, 8 },
                    { 11, "Điện thoại", "Smartphone gập cao cấp màn hình không nếp gấp, Hasselblad Camera flagship, chip Snapdragon 8 Gen 2.", "https://images.unsplash.com/photo-1565849904461-04a58ad377e0?w=500&auto=format&fit=crop&q=80", "OPPO Find N3 Fold 512GB", 38990000m, 4 },
                    { 12, "Điện thoại", "Máy tính bảng siêu mỏng 5.3mm, màn hình Tandem OLED Ultra Retina XDR, chip M4 hiệu năng vượt trội.", "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=500&auto=format&fit=crop&q=80", "iPad Pro 11 inch M4 256GB", 28490000m, 11 },
                    { 13, "Phụ kiện", "Bàn phím cơ không dây Bluetooth/Type-C, Gateron Pro Switch, LED RGB 16.8 triệu màu, tùy biến QMK/VIA.", "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=500&auto=format&fit=crop&q=80", "Bàn phím cơ Keychron K8 Pro", 2450000m, 25 },
                    { 14, "Phụ kiện", "Con lăn cuộn vô tận MagSpeed, cảm biến 8000 DPI theo dõi trên kính, phím bấm chống ồn Quiet Clicks 90%.", "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=500&auto=format&fit=crop&q=80", "Chuột Logitech MX Master 3S", 2190000m, 30 },
                    { 15, "Phụ kiện", "Trạm sạc không dây nam chân xếp gọn cho iPhone, Apple Watch và AirPods, công suất sạc nhanh Qi2 15W.", "https://images.unsplash.com/photo-1586953208448-b95a79798f07?w=500&auto=format&fit=crop&q=80", "Đế sạc Anker MagGo 3-in-1 Qi2", 2390000m, 20 },
                    { 16, "Phụ kiện", "Bộ chuyển đổi đa năng cổng HDMI 4K60Hz, Ethernet 1Gbps, khe thẻ SD/MicroSD, sạc PD 100W vỏ nhôm nguyên khối.", "https://images.unsplash.com/photo-1616440347437-b1c73416efc2?w=500&auto=format&fit=crop&q=80", "Hub USB-C HyperDrive 10-in-1", 1850000m, 18 },
                    { 17, "Phụ kiện", "Bàn phím cơ siêu mỏng thời trang, kết nối 3 chế độ (2.4G 1000Hz, Bluetooth 5.1, Dây), Gateron Low Profile Switch.", "https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?w=500&auto=format&fit=crop&q=80", "Bàn phím NuPhy Air75 V2 Low Profile", 3290000m, 14 },
                    { 18, "Phụ kiện", "Trọng lượng siêu nhẹ 63g, Cảm biến Focus Pro 30K DPI, Switch quang học Razer Gen-3 độ bền 90 triệu lần nhấn.", "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=500&auto=format&fit=crop&q=80", "Chuột Gaming Razer DeathAdder V3 Pro", 3490000m, 16 },
                    { 19, "Âm thanh", "Tai nghe chụp tai chống ồn chủ động ANC hàng đầu thế giới, thời lượng pin 30 giờ, chuẩn âm thanh Hi-Res LDAC.", "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&auto=format&fit=crop&q=80", "Tai nghe Sony WH-1000XM5", 6990000m, 7 },
                    { 20, "Âm thanh", "Thiết kế cổ điển phong cách Vintage cao cấp, âm trường rộng mở, kết nối Bluetooth 5.2 và cổng RCA/AUX 3.5mm.", "https://images.unsplash.com/photo-1545454675-3531b543be5d?w=500&auto=format&fit=crop&q=80", "Loa Bluetooth Marshall Stanmore III", 8990000m, 9 },
                    { 21, "Âm thanh", "Tai nghe nhét tai chống ồn chủ động gấp 2 lần, chip H2, âm thanh không gian Spatial Audio theo dõi đầu người.", "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=500&auto=format&fit=crop&q=80", "Apple AirPods Pro 2 USB-C", 5790000m, 22 },
                    { 22, "Âm thanh", "Loa di động âm thanh JBL Original Pro Sound sống động, pin 20 tiếng, hỗ trợ sạc ngược cho điện thoại qua cổng USB.", "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=500&auto=format&fit=crop&q=80", "Loa JBL Charge 5 Kháng Nước IP67", 3690000m, 14 },
                    { 23, "Âm thanh", "Tai nghe không dây chất âm Audiophile cao cấp, Bluetooth 5.4, aptX Lossless, chống ồn thích ứng Adaptive ANC.", "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=500&auto=format&fit=crop&q=80", "Sennheiser Momentum True Wireless 4", 7490000m, 6 },
                    { 24, "Âm thanh", "Loa dã ngoại vỏ nhôm chống xước đạt chuẩn IP67, quai treo tiện lợi, âm thanh 360 độ âm bass mạnh mẽ.", "https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=500&auto=format&fit=crop&q=80", "Loa B&O Beosound Explore", 4990000m, 8 },
                    { 25, "Đồng hồ", "Đồng hồ thông minh theo dõi sức khỏe chuyên sâu, đo oxy máu SpO2, ECG tim, chip S9 SIP, cử chỉ Double Tap.", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500&auto=format&fit=crop&q=80", "Apple Watch Series 9 GPS 45mm", 9890000m, 18 },
                    { 26, "Đồng hồ", "Đồng hồ thể thao chuyên nghiệp sạc năng lượng mặt trời, định vị GPS đa băng tần, đèn pin LED tích hợp, pin 22 ngày.", "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?w=500&auto=format&fit=crop&q=80", "Garmin Fenix 7 Pro Solar Edition", 21490000m, 7 },
                    { 27, "Màn hình", "Màn hình đồ họa & Gaming Nano IPS 27 inch QHD, tần số quét 180Hz (OC), thời gian phản hồi 1ms GtG, HDR400.", "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=500&auto=format&fit=crop&q=80", "Màn Hình LG UltraGear 27GP850-B 2K 180Hz", 8490000m, 12 },
                    { 28, "Máy ảnh", "Máy ảnh Mirrorless Full-frame 33MP, quay phim 4K 60p 10-bit 4:2:2, lấy nét ánh mắt tự động Real-time Eye AF.", "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=500&auto=format&fit=crop&q=80", "Máy Ảnh Sony Alpha A7 Mark IV (Body)", 54900000m, 4 },
                    { 29, "Khác", "Ghế văn phòng cao cấp nhập khẩu Mỹ, chất liệu lưới Pellicle thoáng khí, tựa lưng PostureFit SL bảo vệ cột sống tối đa.", "https://images.unsplash.com/photo-1580481072645-022f9a6d8310?w=500&auto=format&fit=crop&q=80", "Ghế Công Thái Học Herman Miller Aeron", 36800000m, 5 },
                    { 30, "Khác", "Bộ phát Wifi 6 Gigabit 6 dòng dữ liệu tốc độ 5400Mbps, 6 ăng-ten ngoài công suất cao, bảo vệ HomeShield.", "https://images.unsplash.com/photo-1544197150-b99a580bb7a8?w=500&auto=format&fit=crop&q=80", "Router Wifi 6 TP-Link Archer AX73", 2490000m, 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_OrderId",
                table: "CartItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_ProductId",
                table: "CartItem",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItem");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
