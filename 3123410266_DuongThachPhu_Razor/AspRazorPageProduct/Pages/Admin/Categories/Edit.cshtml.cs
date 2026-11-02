using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin.Categories
{
    public class EditModel : PageModel
    {
        private readonly ICategoryService _service;
        public EditModel(ICategoryService service) => _service = service;
        [BindProperty] public Category Category { get; set; } = new();
        public IActionResult OnGet(int id) { var item = _service.GetById(id); if (item is null) return NotFound(); Category = item; return Page(); }
        public IActionResult OnPost() { if (string.IsNullOrWhiteSpace(Category.Name)) ModelState.AddModelError("Category.Name", "Tên danh mục bắt buộc."); if (!ModelState.IsValid) return Page(); _service.Update(Category); TempData["SuccessMessage"] = "Đã cập nhật danh mục."; return RedirectToPage("Index"); }
    }
}
