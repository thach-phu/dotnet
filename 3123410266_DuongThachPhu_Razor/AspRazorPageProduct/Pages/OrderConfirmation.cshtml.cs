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
            if (!User.IsInRole("Admin") &&
                !string.Equals(Order.CustomerEmail, User.FindFirstValue(ClaimTypes.Email), StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return Page();
        }
    }
}
