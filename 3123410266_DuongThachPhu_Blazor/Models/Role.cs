using Microsoft.AspNetCore.Identity;

namespace BlazorWeb.Models;

public sealed class Role : IdentityRole<int>
{
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}
