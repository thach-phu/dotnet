using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using System.Security.Claims;

namespace AspRazorPageProduct.Pages
{
    public class OrderConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;

        public OrderConfirmationModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order? Order { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Index");
            }

            Order = _orderService.GetOrderById(id);
            if (Order == null)
            {
                return RedirectToPage("/Index");
            }
            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
            if (!User.IsInRole("Admin") && Order.UserId != currentUserId)
                return Forbid();

            return Page();
        }
    }
}
