using Microsoft.AspNetCore.Identity;

namespace AspRazorPageProduct.Models
{
    public class Role : IdentityRole<int>
    {
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
    }
}
