using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC.Migrations
{
    /// <inheritdoc />
    public partial class LegacySchemaBaseline : Migration
    {
        // This migration exists because the app originally bootstrapped its
        // schema via Database.EnsureCreated() instead of migrations, so these
        // 4 tables already exist on every database this app has ever been run
        // against. It uses "IF NOT EXISTS"/"IF EXISTS" raw SQL (rather than
        // the normal CreateTable/DropTable builders) so it is a safe no-op on
        // those already-provisioned databases, while still being able to
        // create this schema from scratch on a brand-new database.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `Categories` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `Name` longtext NOT NULL,
                    PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `Orders` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `OrderCode` longtext NOT NULL,
                    `CustomerName` longtext NOT NULL,
                    `CustomerPhone` longtext NOT NULL,
                    `CustomerAddress` longtext NOT NULL,
                    `Note` longtext NULL,
                    `SubTotal` decimal(18,2) NOT NULL,
                    `ShippingFee` decimal(18,2) NOT NULL,
                    `GrandTotal` decimal(18,2) NOT NULL,
                    `Status` longtext NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `Products` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `Name` longtext NOT NULL,
                    `Category` longtext NOT NULL,
                    `Price` decimal(18,2) NOT NULL,
                    `Description` longtext NOT NULL,
                    `Emoji` longtext NOT NULL,
                    `IsPopular` tinyint(1) NOT NULL,
                    `ImageUrl` longtext NOT NULL,
                    `CategoryId` int NOT NULL,
                    PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
                """);

            // The index is declared inline (rather than as a separate
            // CREATE INDEX statement) so the whole table — index included —
            // is skipped atomically by "IF NOT EXISTS" on a database that
            // already has it, with no separate idempotency check needed.
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `OrderItems` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `OrderId` int NOT NULL,
                    `ProductId` int NOT NULL,
                    `ProductName` longtext NOT NULL,
                    `Price` decimal(18,2) NOT NULL,
                    `Quantity` int NOT NULL,
                    `ImageUrl` longtext NOT NULL,
                    PRIMARY KEY (`Id`),
                    KEY `IX_OrderItems_OrderId` (`OrderId`),
                    CONSTRAINT `FK_OrderItems_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `OrderItems`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `Products`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `Orders`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `Categories`;");
        }
    }
}

