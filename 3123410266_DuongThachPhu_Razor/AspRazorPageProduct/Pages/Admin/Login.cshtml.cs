using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        public LoginModel(IAuthService authService) => _authService = authService;

        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true && Permissions.HasAnyAdminAccess(User))
            {
                return RedirectToPage("/Admin/Dashboard/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác. Vui lòng kiểm tra lại.");
                return Page();
            }

            var principal = await _authService.ValidateAsync(Email, Password);
            if (principal is null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác. Vui lòng kiểm tra lại.");
                return Page();
            }
            if (!Permissions.HasAnyAdminAccess(principal))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản này không có quyền quản trị.");
                return Page();
            }

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }
            return RedirectToPage("/Admin/Dashboard/Index");
        }
    }
}
