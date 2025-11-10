namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión de productos.
    /// </summary>
    public class DimProductRecord
    {
        public int ProductKey { get; set; }  
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
