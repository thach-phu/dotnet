using System.Security.Claims;

namespace AspRazorPageProduct.Services
{
    public interface IAuthService
    {
        Task<ClaimsPrincipal?> ValidateAsync(string email, string password);
    }
}
