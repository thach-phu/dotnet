using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;
        public DetailsModel(IOrderService orderService) => _orderService = orderService;

        public Order Order { get; private set; } = null!;
        public IReadOnlyList<string> ValidNextStatuses { get; private set; } = [];

        public IActionResult OnGet(string id)
        {
            var order = _orderService.GetOrderById(id);
            if (order is null) return NotFound();
            Order = order;
            ValidNextStatuses = _orderService.GetValidNextStatuses(order.Status);
            return Page();
        }

        public IActionResult OnPostUpdateStatus(string orderId, string status)
        {
            var result = _orderService.UpdateStatus(orderId, status);
            TempData[result.Succeeded ? "OrderSuccess" : "OrderError"] =
                result.Succeeded ? "Đã cập nhật trạng thái đơn hàng." : result.ErrorMessage;
            if (!result.Succeeded && _orderService.GetOrderById(orderId) is null)
            {
                // Đơn hàng không còn tồn tại — quay về danh sách thay vì điều hướng
                // tới trang chi tiết (sẽ trả 404 và người dùng không thấy lỗi này).
                return RedirectToPage("/Admin/Orders/Index");
            }
            return RedirectToPage(new { id = orderId });
        }
    }
}
