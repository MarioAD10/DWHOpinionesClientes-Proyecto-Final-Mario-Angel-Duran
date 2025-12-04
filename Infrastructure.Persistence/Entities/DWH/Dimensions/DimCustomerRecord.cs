using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión de clientes.
    /// </summary>
    public class DimCustomerRecord
    {
        [Key]
        public int CustomerKey { get; set; }            
        public string CustomerName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string AgeRange { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
