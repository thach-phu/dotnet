using AspRazorPageProduct.Data;
using AspRazorPageProduct.Models;
using Microsoft.EntityFrameworkCore;

namespace AspRazorPageProduct.Services
{
    public sealed class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;
        public CustomerService(AppDbContext db) => _db = db;

        public List<CustomerListItem> GetAll() =>
            _db.Customers.AsNoTracking()
                .OrderBy(c => c.Id)
                .GroupJoin(_db.Users, c => c.UserId, u => u.Id, (c, users) => new { c, users })
                .SelectMany(x => x.users.DefaultIfEmpty(), (x, u) => new CustomerListItem(x.c.Id, x.c.FullName, x.c.Phone, u == null ? null : u.Email))
                .ToList();

        public (bool Success, string? Error, Customer? Customer) Create(string fullName, string phone)
        {
            fullName = fullName?.Trim() ?? "";
            phone = phone?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(fullName)) return (false, "Vui lòng nhập họ tên.", null);
            if (string.IsNullOrWhiteSpace(phone)) return (false, "Vui lòng nhập số điện thoại.", null);

            var customer = new Customer { FullName = fullName, Phone = phone };
            _db.Customers.Add(customer);
            _db.SaveChanges();
            return (true, null, customer);
        }

        public (bool Success, string? Error) Update(int customerId, string fullName, string phone)
        {
            fullName = fullName?.Trim() ?? "";
            phone = phone?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(fullName)) return (false, "Vui lòng nhập họ tên.");
            if (string.IsNullOrWhiteSpace(phone)) return (false, "Vui lòng nhập số điện thoại.");

            var customer = _db.Customers.Find(customerId);
            if (customer is null) return (false, "Không tìm thấy khách hàng.");

            customer.FullName = fullName;
            customer.Phone = phone;

            // Khách hàng này đã liên kết tài khoản thì đồng bộ luôn tên/SĐT sang tài
            // khoản đó, giống như lúc map lần đầu.
            if (customer.UserId is { } userId)
            {
                var user = _db.Users.Find(userId);
                if (user is not null)
                {
                    user.FullName = fullName;
                    user.PhoneNumber = phone;
                }
            }

            _db.SaveChanges();
            return (true, null);
        }

        public (bool Success, string? Error) Delete(int customerId)
        {
            var customer = _db.Customers.Find(customerId);
            if (customer is null) return (false, "Không tìm thấy khách hàng.");

            _db.Customers.Remove(customer);
            _db.SaveChanges();
            return (true, null);
        }

        public List<User> GetUnlinkedUsers() =>
            _db.Users.AsNoTracking()
                .Where(u => !_db.Customers.Any(c => c.UserId == u.Id))
                .OrderBy(u => u.Email)
                .ToList();

        public (bool Success, string? Error) MapToUser(int customerId, int userId)
        {
            var customer = _db.Customers.Find(customerId);
            if (customer is null) return (false, "Không tìm thấy khách hàng.");
            if (customer.UserId is not null) return (false, "Khách hàng này đã được liên kết với 1 tài khoản.");
            if (_db.Customers.Any(c => c.UserId == userId)) return (false, "Tài khoản này đã được liên kết với khách hàng khác.");

            var user = _db.Users.Find(userId);
            if (user is null) return (false, "Không tìm thấy tài khoản.");

            if (_db.Database.IsRelational())
            {
                // Chặn race: chỉ ghi khi khách hàng vẫn chưa được liên kết tại thời điểm UPDATE,
                // không chỉ tại thời điểm kiểm tra phía trên.
                var updated = _db.Customers
                    .Where(c => c.Id == customerId && c.UserId == null)
                    .ExecuteUpdate(setters => setters.SetProperty(c => c.UserId, userId));
                if (updated == 0)
                {
                    return (false, "Khách hàng này đã được liên kết với 1 tài khoản.");
                }
            }
            else
            {
                customer.UserId = userId;
                _db.SaveChanges();
            }

            // Gán sẵn họ tên/SĐT của khách hàng cho tài khoản vừa liên kết, vì tài khoản
            // admin tạo tay có thể không nhập 2 trường này.
            user.FullName = customer.FullName;
            user.PhoneNumber = customer.Phone;
            _db.SaveChanges();

            return (true, null);
        }
    }
}
