using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages.Admin.Products
{
    public class DeleteModel : PageModel
    {
        private readonly IProductService _productService;

        public DeleteModel(IProductService productService)
        {
            _productService = productService;
        }

        public Product? Product { get; set; }

        public IActionResult OnGet(int id)
        {
            Product = _productService.GetById(id);
            if (Product == null)
            {
                return RedirectToPage("/Admin/Products/Index");
            }
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var product = _productService.GetById(id);
            if (product != null)
            {
                _productService.Delete(id);
                TempData["SuccessMessage"] = $"Đã xóa sản phẩm \"{product.Name}\" thành công!";
            }
            return RedirectToPage("/Admin/Products/Index");
        }
    }
}
