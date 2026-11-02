using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _service;
        public CreateModel(ICategoryService service) => _service = service;
        [BindProperty] public Category Category { get; set; } = new();
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Category.Name)) ModelState.AddModelError("Category.Name", "Tên danh mục bắt buộc.");
            if (!ModelState.IsValid) return Page();
            _service.Add(Category); TempData["SuccessMessage"] = "Đã thêm danh mục."; return RedirectToPage("Index");
        }
    }
}
