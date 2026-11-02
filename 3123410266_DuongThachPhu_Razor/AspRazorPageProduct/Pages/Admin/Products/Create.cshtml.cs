using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;

namespace AspRazorPageProduct.Pages.Admin.Products
{
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;

        public CreateModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public Product Product { get; set; } = new Product();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Product.Name))
            {
                ModelState.AddModelError("Product.Name", "Tên sản phẩm không được để trống!");
            }
            if (Product.Price <= 0)
            {
                ModelState.AddModelError("Product.Price", "Giá sản phẩm phải lớn hơn 0!");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Product.ImageUrl))
            {
                Product.ImageUrl = "https://images.unsplash.com/photo-1526738549149-8e07eca6c147?w=500&auto=format&fit=crop&q=80";
            }

            _productService.Add(Product);
            TempData["SuccessMessage"] = $"Thêm sản phẩm \"{Product.Name}\" thành công!";
            return RedirectToPage("/Admin/Products/Index");
        }
    }
}
