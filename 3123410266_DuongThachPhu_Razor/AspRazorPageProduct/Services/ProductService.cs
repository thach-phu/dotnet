using AspRazorPageProduct.Data;
using AspRazorPageProduct.Helpers;
using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        // LINQ: Lấy toàn bộ danh sách đã qua lọc và sắp xếp
        public List<Product> GetAll(string? search = null, string? category = null, string? sortBy = null)
        {
            return GetFilteredQuery(search, category, sortBy).ToList();
        }

        // LINQ & Phân trang: Phân trang danh sách sản phẩm bằng LINQ Skip và Take
        public PaginatedList<Product> GetPaginated(string? search = null, string? category = null, string? sortBy = null, int pageIndex = 1, int pageSize = 8)
        {
            var query = GetFilteredQuery(search, category, sortBy);
            return PaginatedList<Product>.Create(query, pageIndex, pageSize);
        }

        // Phương thức bổ trợ dùng LINQ Query Syntax & Method Syntax
        private IQueryable<Product> GetFilteredQuery(string? search, string? category, string? sortBy)
        {
            var query = _context.Products.AsQueryable();

            // LINQ Where: Tìm kiếm theo tên hoặc mô tả
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = from p in query
                        where p.Name.ToLower().Contains(keyword) ||
                              p.Description.ToLower().Contains(keyword)
                        select p;
            }

            // LINQ Where: Lọc theo danh mục
            if (!string.IsNullOrWhiteSpace(category) && category != "Tất cả")
            {
                var categoryLower = category.ToLower();
                query = query.Where(p => p.Category.ToLower() == categoryLower);
            }

            // LINQ OrderBy: Sắp xếp kết quả
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.Id)
            };

            return query;
        }

        public Product? GetById(int id)
        {
            return _context.Products.Find(id);
        }

        public List<string> GetCategories()
        {
            // LINQ Select & Distinct & OrderBy
            return _context.Products
                            .Select(p => p.Category)
                            .Distinct()
                            .OrderBy(c => c)
                            .ToList();
        }

        public void Add(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void Update(Product product)
        {
            var existing = _context.Products.Find(product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Category = product.Category;
                existing.ImageUrl = product.ImageUrl;
                existing.Stock = product.Stock;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }
    }
}
