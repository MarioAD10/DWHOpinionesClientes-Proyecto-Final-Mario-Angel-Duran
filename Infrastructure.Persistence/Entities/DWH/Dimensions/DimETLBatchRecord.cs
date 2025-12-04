using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión que almacena información sobre los lotes de carga ETL.
    /// </summary>
    public class DimETLBatchRecord
    {
        [Key]
        public int ETLBatchKey { get; set; }              
        public string BatchName { get; set; } = string.Empty;
        public DateTime LoadDate { get; set; }
        public string SourceFile { get; set; } = string.Empty;
    }
}
