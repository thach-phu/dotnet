using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages.Admin.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;

        public EditModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public Product Product { get; set; } = new Product();

        public IActionResult OnGet(int id)
        {
            var product = _productService.GetById(id);
            if (product == null)
            {
                return RedirectToPage("/Admin/Products/Index");
            }
            Product = product;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _productService.Update(Product);
            TempData["SuccessMessage"] = $"Cập nhật sản phẩm \"{Product.Name}\" thành công!";
            return RedirectToPage("/Admin/Products/Index");
        }
    }
}
