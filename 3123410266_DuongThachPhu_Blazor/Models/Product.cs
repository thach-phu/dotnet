namespace BlazorWeb.Models;

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
    public int StockQuantity { get; set; } = 100;
}
