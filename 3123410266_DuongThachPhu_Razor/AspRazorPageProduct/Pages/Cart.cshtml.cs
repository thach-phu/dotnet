using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages
{
    public class CartModel : PageModel
    {
        private readonly ICartService _cartService;

        public CartModel(ICartService cartService)
        {
            _cartService = cartService;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal GrandTotal { get; set; }

        public void OnGet()
        {
            CartItems = _cartService.GetCart();
            GrandTotal = _cartService.GetGrandTotal();
        }

        public IActionResult OnPostUpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToPage();
        }

        public IActionResult OnPostRemoveItem(int productId)
        {
            _cartService.RemoveFromCart(productId);
            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToPage();
        }

        public IActionResult OnPostClearCart()
        {
            _cartService.ClearCart();
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToPage();
        }
    }
}
