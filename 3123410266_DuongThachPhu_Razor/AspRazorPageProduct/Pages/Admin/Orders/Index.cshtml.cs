using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public List<Order> Orders { get; set; } = new();
        public decimal TotalRevenue { get; set; }

        public void OnGet()
        {
            Orders = _orderService.GetAllOrders();
            TotalRevenue = _orderService.GetDashboardSummary().TotalRevenue;
        }

        public IActionResult OnPostCancelOrder(string orderId)
        {
            var result = _orderService.UpdateStatus(orderId, "Đã hủy");
            TempData[result.Succeeded ? "OrderSuccess" : "OrderError"] =
                result.Succeeded ? "Đã hủy đơn hàng." : result.ErrorMessage;
            return RedirectToPage();
        }
    }
}
