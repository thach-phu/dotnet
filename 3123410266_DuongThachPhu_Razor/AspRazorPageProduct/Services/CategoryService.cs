using AspRazorPageProduct.Data;
using AspRazorPageProduct.Models;
using Microsoft.EntityFrameworkCore;

namespace AspRazorPageProduct.Services
{
    public sealed class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        public CategoryService(AppDbContext context) => _context = context;
        public List<Category> GetAll() => _context.Categories.OrderBy(c => c.Name).ToList();
        public Category? GetById(int id) => _context.Categories.Find(id);
        public void Add(Category category) { category.Name = category.Name.Trim(); _context.Categories.Add(category); _context.SaveChanges(); }
        public void Update(Category category)
        {
            var existing = GetById(category.Id);
            if (existing is null) return;
            existing.Name = category.Name.Trim();
            existing.Description = category.Description;
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var category = GetById(id);
            if (category is null) return;
            if (_context.Products.Any(p => p.Category == category.Name))
                throw new InvalidOperationException("Không thể xóa danh mục đang có sản phẩm.");
            _context.Categories.Remove(category);
            _context.SaveChanges();
        }
    }
}
