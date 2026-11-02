using System.Text.Json;
using MVC.Models;

namespace MVC.Services;

public sealed class SessionCart(IHttpContextAccessor httpContextAccessor, ProductCatalog catalog)
{
    private const string Key = "mvc-cart";
    private IDictionary<int, int> Items
    {
        get
        {
            var json = httpContextAccessor.HttpContext?.Session.GetString(Key);
            return string.IsNullOrWhiteSpace(json)
                ? new Dictionary<int, int>()
                : JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? [];
        }
    }

    public CartViewModel GetCart()
    {
        var lines = Items.Select(pair => (Product: catalog.GetById(pair.Key), pair.Value))
            .Where(item => item.Product is not null)
            .Select(item => new CartLineViewModel(item.Product!, item.Value))
            .ToList();
        return new CartViewModel { Items = lines };
    }

    public void Add(int productId, int quantity = 1)
    {
        var items = Items;
        items.TryGetValue(productId, out var currentQuantity);
        items[productId] = Math.Clamp(currentQuantity + quantity, 1, 20);
        Save(items);
    }

    public void Remove(int productId)
    {
        var items = Items;
        items.Remove(productId);
        Save(items);
    }

    public void Update(int productId, int quantity)
    {
        if (quantity <= 0) Remove(productId);
        else
        {
            var items = Items;
            items[productId] = Math.Min(quantity, 20);
            Save(items);
        }
    }

    public void Clear() => Save(new Dictionary<int, int>());

    private void Save(IDictionary<int, int> items) =>
        httpContextAccessor.HttpContext?.Session.SetString(Key, JsonSerializer.Serialize(items));
}
