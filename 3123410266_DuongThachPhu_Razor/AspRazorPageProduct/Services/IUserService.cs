using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public interface IUserService
    {
        List<User> GetAllWithRoles();
        User? GetById(int id);
        int? GetRoleId(int userId);
        Task<(bool Success, string? Error)> UpdateRoleAsync(int userId, int roleId);
        Task<(bool Success, string? Error, User? User)> CreateAccountAsync(
            string? fullName, string? phone, string email, string password, int roleId);
        Task<(bool Success, string? Error)> DeleteUserAsync(int userId);
        Task<(bool Success, string? Error)> SetUserLockoutAsync(int userId, bool disabled);
    }
}
