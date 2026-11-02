using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        public LoginModel(IAuthService authService) => _authService = authService;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
        public void OnGet() { }
        public async Task<IActionResult> OnPostAsync()
        {
            var principal = await _authService.ValidateAsync(Email, Password);
            if (principal is null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return Page();
            }
            if (Permissions.HasAnyAdminAccess(principal))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản quản trị vui lòng đăng nhập tại trang quản trị (/admin/login).");
                return Page();
            }

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

            if (!string.IsNullOrWhiteSpace(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }
            return LocalRedirect(Url.Page("/Index")!);
        }
    }
}
