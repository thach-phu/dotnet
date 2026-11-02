using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public DetailsModel(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public Product? Product { get; set; }

        [BindProperty]
        public int Quantity { get; set; } = 1;

        public IActionResult OnGet(int id)
        {
            Product = _productService.GetById(id);
            if (Product == null)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public IActionResult OnPostAddToCart(int id)
        {
            Product = _productService.GetById(id);
            if (Product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại!";
                return RedirectToPage("/Index");
            }

            if (Quantity < 1) Quantity = 1;
            if (Quantity > Product.Stock) Quantity = Product.Stock;

            _cartService.AddToCart(id, Quantity);
            TempData["SuccessMessage"] = $"Đã thêm {Quantity} x \"{Product.Name}\" vào giỏ hàng!";
            return RedirectToPage("/Cart");
        }
    }
}
