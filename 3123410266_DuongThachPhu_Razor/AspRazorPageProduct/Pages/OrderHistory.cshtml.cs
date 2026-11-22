using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AspRazorPageProduct.Pages
{
    [Authorize]
    public class OrderHistoryModel : PageModel
    {
        private readonly IOrderService _orders;
        public OrderHistoryModel(IOrderService orders) => _orders = orders;
        public List<Order> Orders { get; private set; } = [];
        public void OnGet() => Orders = _orders.GetOrdersForCustomer(GetCurrentUserId());

        public IActionResult OnPostCancelOrder(string orderId)
        {
            var result = _orders.CancelOrderByCustomer(orderId, GetCurrentUserId());
            if (!result.Succeeded)
            {
                TempData["CancelError"] = result.ErrorMessage;
            }
            return RedirectToPage();
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
