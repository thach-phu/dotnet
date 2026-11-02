using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ICustomerService _customers;
        public IndexModel(ICustomerService customers) => _customers = customers;

        public List<CustomerListItem> Customers { get; private set; } = [];
        public List<User> UnlinkedUsers { get; private set; } = [];

        [BindProperty] public string FullName { get; set; } = "";
        [BindProperty] public string Phone { get; set; } = "";

        public void OnGet() => Load();

        public IActionResult OnPostCreate()
        {
            var result = _customers.Create(FullName, Phone);
            if (!result.Success) TempData["CustomerError"] = result.Error;
            return RedirectToPage();
        }

        public IActionResult OnPostMap(int customerId, int userId)
        {
            var result = _customers.MapToUser(customerId, userId);
            if (!result.Success) TempData["CustomerError"] = result.Error;
            return RedirectToPage();
        }

        public IActionResult OnPostUpdate(int customerId, string fullName, string phone)
        {
            var result = _customers.Update(customerId, fullName, phone);
            if (result.Success) TempData["CustomerSuccess"] = "Đã cập nhật khách hàng.";
            else TempData["CustomerError"] = result.Error;
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int customerId)
        {
            var result = _customers.Delete(customerId);
            if (result.Success) TempData["CustomerSuccess"] = "Đã xóa khách hàng.";
            else TempData["CustomerError"] = result.Error;
            return RedirectToPage();
        }

        private void Load()
        {
            Customers = _customers.GetAll();
            UnlinkedUsers = _customers.GetUnlinkedUsers();
        }
    }
}
