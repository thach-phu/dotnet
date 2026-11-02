namespace AspRazorPageProduct.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public int? UserId { get; set; }
    }
}
