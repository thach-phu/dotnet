using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _service;
        public IndexModel(ICategoryService service) => _service = service;
        public List<Category> Categories { get; private set; } = [];
        public void OnGet() => Categories = _service.GetAll();
        public IActionResult OnPostDelete(int id)
        {
            try { _service.Delete(id); TempData["SuccessMessage"] = "Đã xóa danh mục."; }
            catch (InvalidOperationException ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToPage();
        }
    }
}
