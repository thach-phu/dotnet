using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using System.Security.Claims;

namespace AspRazorPageProduct.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public CheckoutModel(
            ICartService cartService,
            IOrderService orderService,
            IProductService productService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _productService = productService;
        }

        [BindProperty]
        public Order Order { get; set; } = new Order();

        public List<CartItem> CartItems { get; set; } = new();
        public decimal GrandTotal { get; set; }

        public IActionResult OnGet()
        {
            CartItems = _cartService.GetCart();
            if (!CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToPage("/Cart");
            }

            GrandTotal = _cartService.GetGrandTotal();
            Order.CustomerEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            Order.CustomerName = User.Identity?.Name ?? string.Empty;
            return Page();
        }

        public IActionResult OnPost()
        {
            CartItems = _cartService.GetCart();
            if (!CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToPage("/Cart");
            }

            if (string.IsNullOrWhiteSpace(Order.CustomerName) ||
                string.IsNullOrWhiteSpace(Order.CustomerPhone) ||
                string.IsNullOrWhiteSpace(Order.ShippingAddress))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin giao hàng!");
                GrandTotal = _cartService.GetGrandTotal();
                return Page();
            }

            var quantitiesByProduct = CartItems
                .GroupBy(item => item.ProductId)
                .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

            foreach (var item in CartItems)
            {
                if (item.ProductId <= 0 || item.Quantity <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Giỏ hàng có sản phẩm hoặc số lượng không hợp lệ.");
                    TempData["ErrorMessage"] = "Không thể đặt hàng vì giỏ hàng có sản phẩm hoặc số lượng không hợp lệ.";
                    GrandTotal = _cartService.GetGrandTotal();
                    return Page();
                }
            }

            foreach (var quantity in quantitiesByProduct)
            {
                var product = _productService.GetById(quantity.Key);
                if (product is null)
                {
                    ModelState.AddModelError(string.Empty, "Một sản phẩm trong giỏ hàng không còn tồn tại.");
                    TempData["ErrorMessage"] = "Không thể đặt hàng vì một sản phẩm trong giỏ hàng không còn tồn tại.";
                    GrandTotal = _cartService.GetGrandTotal();
                    return Page();
                }

                if (product.Stock < quantity.Value)
                {
                    var errorMessage =
                        $"Sản phẩm “{product.Name}” chỉ còn {product.Stock} trong kho, " +
                        $"nhưng giỏ hàng yêu cầu {quantity.Value}. Vui lòng cập nhật giỏ hàng.";
                    ModelState.AddModelError(string.Empty, errorMessage);
                    TempData["ErrorMessage"] = errorMessage;
                    GrandTotal = _cartService.GetGrandTotal();
                    return Page();
                }
            }

            Order.Items = new List<CartItem>(CartItems);
            Order.CustomerEmail = User.FindFirstValue(ClaimTypes.Email) ?? Order.CustomerEmail;
            Order.TotalAmount = _cartService.GetGrandTotal();
            
            _orderService.CreateOrder(Order);
            _cartService.ClearCart();

            return RedirectToPage("/OrderConfirmation", new { id = Order.OrderId });
        }
    }
}
