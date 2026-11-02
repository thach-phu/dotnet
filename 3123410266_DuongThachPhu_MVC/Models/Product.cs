namespace MVC.Models;

public sealed record Product(
    int Id,
    string Name,
    string Category,
    decimal Price,
    string Description,
    string Emoji,
    bool IsPopular = false,
    string ImageUrl = "")
{
    public int CategoryId { get; init; }
    public int Stock { get; init; } = 100;
}
