using MVC.Data;
using MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

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
            product with { CategoryId = GetCategoryId(product.Category) });
        db.SaveChanges();
        return true;
    }

    public (bool Success, string? Error, Category? Category) AddCategoryForAdmin(string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return (false, "Tên danh mục không được để trống.", null);
        if (db.Categories.Any(c => c.Name.ToLower() == name.ToLower()))
            return (false, "Danh mục đã tồn tại.", null);
        var category = AddCategory(name);
        return (true, null, category);
    }

    public (bool Success, string? Error) UpdateCategory(int id, string name)
    {
        name = name.Trim();
        var category = db.Categories.Find(id);
        if (category is null) return (false, "Không tìm thấy danh mục.");
        if (string.IsNullOrWhiteSpace(name)) return (false, "Tên danh mục không được để trống.");
        if (db.Categories.Any(c => c.Id != id && c.Name.ToLower() == name.ToLower()))
            return (false, "Danh mục đã tồn tại.");
        db.Entry(category).CurrentValues.SetValues(new Category(id, name));
        db.Products.Where(p => p.CategoryId == id).ExecuteUpdate(s => s.SetProperty(p => p.Category, name));
        db.SaveChanges();
        return (true, null);
    }

    public (bool Success, string? Error) RemoveCategory(int id)
    {
        var category = db.Categories.Find(id);
        if (category is null) return (false, "Không tìm thấy danh mục.");
        if (db.Products.Any(p => p.CategoryId == id))
            return (false, "Không thể xóa danh mục đang có sản phẩm.");
        db.Categories.Remove(category);
        db.SaveChanges();
        return (true, null);
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
