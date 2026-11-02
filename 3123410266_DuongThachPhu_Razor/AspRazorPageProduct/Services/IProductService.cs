using AspRazorPageProduct.Helpers;
using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public interface IProductService
    {
        List<Product> GetAll(string? search = null, string? category = null, string? sortBy = null);
        PaginatedList<Product> GetPaginated(string? search = null, string? category = null, string? sortBy = null, int pageIndex = 1, int pageSize = 8);
        Product? GetById(int id);
        List<string> GetCategories();
        void Add(Product product);
        void Update(Product product);
        void Delete(int id);
    }
}
