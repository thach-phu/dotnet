using BlazorWeb.Data;
using BlazorWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorWeb.Services;

public sealed class CategoryService(StoreDbContext db)
{
    public List<Category> GetAll() => db.Categories
        .AsNoTracking()
        .OrderBy(category => category.Name)
        .ToList();

    public (bool Success, string? Error, Category? Category) Create(string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Vui lòng nhập tên danh mục.", null);
        if (db.Categories.Any(category => category.Name.ToLower() == name.ToLower()))
            return (false, "Danh mục này đã tồn tại.", null);

        var category = new Category((db.Categories.Max(category => (int?)category.Id) ?? 0) + 1, name);
        db.Categories.Add(category);
        db.SaveChanges();
        return (true, null, category);
    }

    public (bool Success, string? Error) Update(int id, string name)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Vui lòng nhập tên danh mục.");
        if (db.Categories.Any(category => category.Id != id && category.Name.ToLower() == name.ToLower()))
            return (false, "Danh mục này đã tồn tại.");

        var category = db.Categories.Find(id);
        if (category is null) return (false, "Không tìm thấy danh mục.");

        var products = db.Products.Where(product => product.CategoryId == id).ToList();
        db.Entry(category).Property(item => item.Name).CurrentValue = name;
        foreach (var product in products)
            db.Entry(product).Property(item => item.Category).CurrentValue = name;
        db.SaveChanges();
        return (true, null);
    }

    public (bool Success, string? Error) Delete(int id)
    {
        var category = db.Categories.Find(id);
        if (category is null) return (false, "Không tìm thấy danh mục.");
        if (db.Products.Any(product => product.CategoryId == id))
            return (false, "Không thể xóa danh mục đang có sản phẩm. Hãy chuyển hoặc xóa sản phẩm trước.");

        db.Categories.Remove(category);
        db.SaveChanges();
        return (true, null);
    }
}
