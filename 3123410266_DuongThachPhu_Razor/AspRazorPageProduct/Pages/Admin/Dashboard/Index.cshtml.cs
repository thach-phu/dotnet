using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        public IndexModel(IOrderService orderService) => _orderService = orderService;

        public DashboardSummary Summary { get; private set; } = new(0, new(0, 0, 0, 0, 0));

        public IActionResult OnGet()
        {
            if (!Permissions.HasAnyAdminAccess(User))
            {
                return Forbid();
            }

            Summary = _orderService.GetDashboardSummary();
            return Page();
        }
    }
}
