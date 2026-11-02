using System.Security.Claims;

namespace AspRazorPageProduct.Models
{
    public static class Permissions
    {
        public const string ClaimType = "permission";
        public const string ProductsManage = "products.manage";
        public const string OrdersManage = "orders.manage";
        public const string UsersManage = "users.manage";
        public const string CategoriesManage = "categories.manage";
        public const string CustomersManage = "customers.manage";

        public static readonly IReadOnlyList<(string Code, string Description)> Catalog =
        [
            (ProductsManage, "Quản lý sản phẩm (thêm/sửa/xóa)"),
            (OrdersManage, "Quản lý đơn hàng (xem/cập nhật trạng thái/xóa)"),
            (UsersManage, "Quản lý người dùng, vai trò và phân quyền"),
            (CategoriesManage, "Quản lý danh mục sản phẩm"),
            (CustomersManage, "Quản lý khách hàng"),
        ];

        public static readonly IReadOnlyList<string> All = Catalog.Select(c => c.Code).ToList();

        public static bool HasAnyAdminAccess(ClaimsPrincipal user) =>
            All.Any(code => user.HasClaim(ClaimType, code));
    }
}
