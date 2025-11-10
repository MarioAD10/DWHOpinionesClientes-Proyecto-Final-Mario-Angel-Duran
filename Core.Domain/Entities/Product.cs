namespace Core.Domain.Entities
{
    public class Product
    {
        public int ProductKey { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
