using System.ComponentModel.DataAnnotations;
using AspRazorPageProduct.Models;
using AspRazorPageProduct.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspRazorPageProduct.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IUserClaimsPrincipalFactory<User> _claimsFactory;

        public RegisterModel(IUserService userService, IUserClaimsPrincipalFactory<User> claimsFactory)
        {
            _userService = userService;
            _claimsFactory = claimsFactory;
        }

        [BindProperty, Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string Phone { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Vui lòng nhập mật khẩu."), MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty, Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userService.RegisterAsync(FullName, Phone, Email, Password);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return Page();
            }

            var principal = await _claimsFactory.CreateAsync(result.User!);
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

            return LocalRedirect(Url.Page("/Index")!);
        }
    }
}
