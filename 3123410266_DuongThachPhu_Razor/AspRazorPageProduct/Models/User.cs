using Microsoft.AspNetCore.Identity;

namespace AspRazorPageProduct.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
