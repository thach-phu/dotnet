using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlazorWeb.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductCatalogData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "iPhone 11" },
                    { 2, "iPhone 12" },
                    { 3, "iPhone 13" },
                    { 4, "iPhone 14" },
                    { 5, "iPhone 15" },
                    { 6, "iPhone 16" },
                    { 7, "iPhone 17" },
                    { 8, "iPhone 18" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CategoryId", "Description", "Emoji", "ImageUrl", "IsPopular", "Name", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 1101, "iPhone 11", 1, "Camera kép, chip A13 Bionic, màn hình Liquid Retina 6.1 inch.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-11/iphone-11-15_1.jpg", false, "iPhone 11 128GB", 8990000m, 100 },
                    { 1102, "iPhone 11", 1, "Màn hình Super Retina XDR, camera Pro và pin dùng cả ngày.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-11/iPhone-11-5.jpg", false, "iPhone 11 Pro Max 64GB", 10990000m, 100 },
                    { 1201, "iPhone 12", 2, "Thiết kế cạnh phẳng, màn hình Super Retina XDR và 5G.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-12/iphone-12-1.jpg", false, "iPhone 12 64GB", 11990000m, 100 },
                    { 1202, "iPhone 12", 2, "Màn hình lớn, camera Pro với LiDAR và hiệu năng A14 Bionic.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-12/iphone-12-7.jpg", false, "iPhone 12 Pro Max 128GB", 11590000m, 100 },
                    { 1301, "iPhone 13", 3, "Chip A15 Bionic, camera kép và thời lượng pin tốt.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-513339.jpg", false, "iPhone 13 128GB", 14990000m, 100 },
                    { 1302, "iPhone 13", 3, "Màn hình ProMotion 120Hz, camera Pro và pin bền bỉ.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-507654.jpg", true, "iPhone 13 Pro Max 128GB", 19990000m, 100 },
                    { 1401, "iPhone 14", 4, "Hiệu năng ổn định, camera kép sắc nét, phát hiện va chạm.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-590067.jpg", false, "iPhone 14 128GB", 18990000m, 100 },
                    { 1402, "iPhone 14", 4, "Dynamic Island, camera chính 48MP và màn hình Always-On.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-403207.jpg", true, "iPhone 14 Pro Max 128GB", 24990000m, 100 },
                    { 1501, "iPhone 15", 5, "Dynamic Island, camera chính 48MP và cổng USB-C.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-663096.png", true, "iPhone 15 128GB", 19990000m, 100 },
                    { 1502, "iPhone 15", 5, "Titan bền nhẹ, chip A17 Pro, camera tele 5x.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-288083.jpg", true, "iPhone 15 Pro Max 256GB", 29990000m, 100 },
                    { 1601, "iPhone 16", 6, "Chip A18, Apple Intelligence và thiết kế gọn gàng.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-326571.webp", false, "iPhone 16e 128GB", 14290000m, 100 },
                    { 1602, "iPhone 16", 6, "Chip A18, nút Camera Control và hệ thống camera kép.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/a-465580.jpg", true, "iPhone 16 128GB", 22990000m, 100 },
                    { 1603, "iPhone 16", 6, "Titan, A18 Pro, màn hình ProMotion và camera Fusion 48MP.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/dien-thoai-iphone-16-pro-max-256gb-410296.jpg", true, "iPhone 16 Pro Max 256GB", 30990000m, 100 },
                    { 1701, "iPhone 17", 7, "Thiết kế hiện đại, dung lượng 256GB và hiệu năng mạnh mẽ.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-141.jpg", false, "iPhone 17e 256GB", 20990000m, 100 },
                    { 1702, "iPhone 17", 7, "Thế hệ iPhone mới với thiết kế tinh tế và hiệu năng vượt trội.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-7_1.jpg", true, "iPhone 17 256GB", 25490000m, 100 },
                    { 1703, "iPhone 17", 7, "Dòng Pro hiệu năng cao, camera chuyên nghiệp và màn hình mượt.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iPhone-17/iphone-17-32_2.jpg", true, "iPhone 17 Pro 256GB", 31990000m, 100 },
                    { 1704, "iPhone 17", 7, "Bản Pro Max cao cấp với camera và thời lượng pin hàng đầu.", "📱", "https://media-asset.cellphones.com.vn/dashboard-v2/uploads/content-uploads/iphone-17-pro-max-256gb-chinh-hang-301444.jpg", true, "iPhone 17 Pro Max 256GB", 34990000m, 100 },
                    { 1801, "iPhone 18", 8, "Mẫu iPhone Pro thế hệ mới, thông tin và giá tham khảo.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-18/iphone-18-pro/iphone-18-pro.jpg", true, "iPhone 18 Pro 256GB", 38990000m, 100 },
                    { 1802, "iPhone 18", 8, "Mẫu Pro Max cao cấp, thông tin và giá tham khảo.", "📱", "https://cdn2.cellphones.com.vn/insecure/rs:fill:0:0/q:100/plain/https://cellphones.com.vn/media/wysiwyg/Phone/Apple/iphone-18/iphone-18-pro/iphone-18-pro_10_.jpg", true, "iPhone 18 Pro Max 256GB", 41990000m, 100 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1101);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1102);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1201);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1202);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1301);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1302);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1401);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1402);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1501);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1502);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1601);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1602);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1603);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1701);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1702);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1703);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1704);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1801);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1802);
        }
    }
}
