using System.Text.Json;
using BlazorWeb.Models;
using Microsoft.JSInterop;

namespace BlazorWeb.Services;

public sealed class CartService(ProductCatalog catalog)
{
    private const int MaxQuantityPerItem = 20;

    private readonly Dictionary<int, (Product Product, int Quantity)> items = [];
    private IJSObjectReference? cookieModule;

    public event Action? Changed;

    public IReadOnlyCollection<(Product Product, int Quantity)> Items => items.Values;

    public int ItemCount => items.Values.Sum(item => item.Quantity);

    public decimal Total => items.Values.Sum(item => item.Product.Price * item.Quantity);
    public decimal Shipping => Total == 0 || Total >= 500000 ? 0 : 30000;
    public decimal GrandTotal => Total + Shipping;

    public async Task AttachAsync(IJSRuntime jsRuntime)
    {
        if (cookieModule is not null) return;
        cookieModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/cart-cookie.js");

        string? json;
        try
        {
            json = await cookieModule.InvokeAsync<string?>("getCart");
        }
        catch (JSException)
        {
            return;
        }

        if (string.IsNullOrEmpty(json)) return;

        List<CartCookieEntry>? entries;
        try
        {
            entries = JsonSerializer.Deserialize<List<CartCookieEntry>>(json);
        }
        catch (JsonException)
        {
            return;
        }

        foreach (var entry in entries ?? [])
        {
            var product = catalog.Products.FirstOrDefault(p => p.Id == entry.ProductId);
            if (product is null) continue;

            var quantity = Math.Clamp(entry.Quantity, 1, MaxQuantityPerItem);
            items[product.Id] = (product, quantity);
        }
        Changed?.Invoke();
    }

    public void Add(Product product, int quantity = 1)
    {
        if (quantity <= 0) return;

        if (items.TryGetValue(product.Id, out var item))
        {
            items[product.Id] = (item.Product, Math.Clamp(item.Quantity + quantity, 1, MaxQuantityPerItem));
        }
        else
        {
            items[product.Id] = (product, Math.Clamp(quantity, 1, MaxQuantityPerItem));
        }

        Changed?.Invoke();
        Persist();
    }

    public void Remove(Product product)
    {
        if (!items.TryGetValue(product.Id, out var item))
        {
            return;
        }

        if (item.Quantity <= 1)
        {
            items.Remove(product.Id);
        }
        else
        {
            items[product.Id] = (item.Product, item.Quantity - 1);
        }

        Changed?.Invoke();
        Persist();
    }

    public void RemoveItem(int productId)
    {
        if (items.Remove(productId))
        {
            Changed?.Invoke();
            Persist();
        }
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        if (!items.TryGetValue(productId, out var item))
        {
            return;
        }

        if (quantity <= 0)
        {
            items.Remove(productId);
        }
        else
        {
            items[productId] = (item.Product, Math.Clamp(quantity, 1, MaxQuantityPerItem));
        }

        Changed?.Invoke();
        Persist();
    }

    public void Clear()
    {
        items.Clear();
        Changed?.Invoke();
        Persist();
    }

    private async void Persist()
    {
        if (cookieModule is null) return;

        try
        {
            var entries = items.Values.Select(v => new CartCookieEntry(v.Product.Id, v.Quantity)).ToList();
            await cookieModule.InvokeVoidAsync("setCart", JsonSerializer.Serialize(entries));
        }
        catch (JSDisconnectedException)
        {
            // Circuit went away mid-write (e.g. browser tab closed) — best-effort only.
        }
        catch (JSException)
        {
            // Best-effort cookie write; the in-memory cart for this circuit is unaffected.
        }
    }

    private sealed record CartCookieEntry(int ProductId, int Quantity);
}
