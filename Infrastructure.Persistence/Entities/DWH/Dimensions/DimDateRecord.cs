using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión de fechas para análisis temporal.
    /// </summary>
    public class DimDateRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DateKey { get; set; }                   
        public DateTime FullDate { get; set; }
        public byte Day { get; set; }
        public byte Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public byte Quarter { get; set; }
        public int Year { get; set; }
    }
}
