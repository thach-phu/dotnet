using Microsoft.AspNetCore.Identity;

namespace MVC.Models;

public sealed class Role : IdentityRole<int>
{
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}
