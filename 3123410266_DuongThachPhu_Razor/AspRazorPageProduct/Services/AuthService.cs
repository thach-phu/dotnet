using System.Security.Claims;
using AspRazorPageProduct.Models;
using Microsoft.AspNetCore.Identity;

namespace AspRazorPageProduct.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserClaimsPrincipalFactory<User> _claimsFactory;

        public AuthService(UserManager<User> userManager, IUserClaimsPrincipalFactory<User> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task<ClaimsPrincipal?> ValidateAsync(string email, string password)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);
            if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return null;
            }
            if (await _userManager.IsLockedOutAsync(user))
            {
                return null;
            }

            return await _claimsFactory.CreateAsync(user);
        }
    }
}
