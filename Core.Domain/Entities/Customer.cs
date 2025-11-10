namespace Core.Domain.Entities
{
    public class Customer
    {
        public int CustomerKey { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
