using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión de sentimientos asociados a opiniones.
    /// </summary>
    public class DimSentimentRecord
    {
        [Key]
        public int SentimentKey { get; set; }    
        public string SentimentName { get; set; } = string.Empty;
        public int Polarity { get; set; }  
    }
}
