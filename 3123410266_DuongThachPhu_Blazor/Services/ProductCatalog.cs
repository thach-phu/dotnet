using BlazorWeb.Data;
using BlazorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorWeb.Services;

public sealed class ProductCatalog
{
    private readonly StoreDbContext db;

    public ProductCatalog(StoreDbContext db)
    {
        this.db = db;
    }

    public List<Product> Products => db.Products.AsNoTracking().ToList();

    public IReadOnlyList<Category> Categories => db.Categories
        .AsNoTracking()
        .OrderBy(category => category.Id)
        .ToList();

    public IEnumerable<Product> GetProductsByCategory(int categoryId) =>
        from product in db.Products.AsNoTracking()
        join category in db.Categories.AsNoTracking()
            on product.CategoryId equals category.Id
        where category.Id == categoryId
        select product;

    public Product? GetById(int id) => db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);

    public List<Product> GetRelatedProducts(int id, string category, int count = 4) =>
        db.Products.AsNoTracking()
            .Where(p => p.Id != id && p.Category == category)
            .Take(count)
            .ToList();

    public void Add(Product product)
    {
        var targetId = product.Id > 0 ? product.Id : ((db.Products.Max(p => (int?)p.Id) ?? 1000) + 1);
        db.Products.Add(product with { Id = targetId, CategoryId = GetCategoryId(product.Category) });
        db.SaveChanges();
    }

    public bool Remove(int id)
    {
        var product = db.Products.Find(id);
        if (product is null)
        {
            return false;
        }

        db.Products.Remove(product);
        db.SaveChanges();
        return true;
    }

    public bool Update(Product product)
    {
        var existing = db.Products.Find(product.Id);
        if (existing is null)
        {
            return false;
        }

        db.Entry(existing).CurrentValues.SetValues(
            product with
            {
                CategoryId = GetCategoryId(product.Category),
                StockQuantity = existing.StockQuantity
            });
        db.SaveChanges();
        return true;
    }

    private int GetCategoryId(string categoryName) =>
        db.Categories.FirstOrDefault(category =>
            string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase))?.Id
        ?? AddCategory(categoryName).Id;

    private Category AddCategory(string name)
    {
        var category = new Category((db.Categories.Max(category => (int?)category.Id) ?? 0) + 1, name);
        db.Categories.Add(category);
        db.SaveChanges();
        return category;
    }
}
