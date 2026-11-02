using Microsoft.AspNetCore.Identity;

namespace MVC.Models;

public sealed class User : IdentityUser<int>
{
    public string FullName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
