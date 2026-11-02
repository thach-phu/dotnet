namespace BlazorWeb.Models;

public sealed class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
}
