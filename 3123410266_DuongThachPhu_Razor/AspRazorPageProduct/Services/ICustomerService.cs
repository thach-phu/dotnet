using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public interface ICustomerService
    {
        List<CustomerListItem> GetAll();
        (bool Success, string? Error, Customer? Customer) Create(string fullName, string phone);
        List<User> GetUnlinkedUsers();
        (bool Success, string? Error) MapToUser(int customerId, int userId);
        (bool Success, string? Error) Update(int customerId, string fullName, string phone);
        (bool Success, string? Error) Delete(int customerId);
    }

    public record CustomerListItem(int Id, string FullName, string Phone, string? LinkedEmail);
}
