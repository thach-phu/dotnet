using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public interface IRoleService
    {
        List<Role> GetRoles();
        List<string> GetPermissionCodes(int roleId);
        List<Permission> GetPermissionCatalog();
        Task<(bool Success, string? Error, Role? Role)> CreateRoleAsync(string name, string? description);
        Task<(bool Success, string? Error)> DeleteRoleAsync(int roleId);
        void SetRolePermissions(int roleId, IReadOnlyCollection<string> permissionCodes);
    }
}
