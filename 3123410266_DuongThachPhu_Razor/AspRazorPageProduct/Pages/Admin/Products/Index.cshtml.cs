using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Helpers;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public PaginatedList<Product> Products { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public void OnGet()
        {
            if (PageIndex < 1) PageIndex = 1;
            Products = _productService.GetPaginated(Search, null, null, PageIndex, PageSize);
        }
    }
}
