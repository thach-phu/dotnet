using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Helpers;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public IndexModel(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public PaginatedList<Product> Products { get; set; } = null!;
        public List<string> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 8;

        public void OnGet()
        {
            // Kiểm tra và gán mặc định nếu nhận giá trị <= 0 từ model binding
            if (PageIndex < 1) PageIndex = 1;
            if (PageSize <= 0) PageSize = 8;

            Categories = _productService.GetCategories();

            // Gọi LINQ phân trang từ ProductService
            Products = _productService.GetPaginated(Search, Category, SortBy, PageIndex, PageSize);
        }

        public IActionResult OnPostAddToCart(int id)
        {
            var product = _productService.GetById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại!";
                return RedirectToPage();
            }

            _cartService.AddToCart(id, 1);
            TempData["SuccessMessage"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng thành công!";
            return RedirectToPage(new { Search, Category, SortBy, PageIndex, PageSize });
        }
    }
}