using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión de canales (web, móvil, redes sociales, etc.).
    /// </summary>
    public class DimChannelRecord
    {
        [Key]
        public int ChannelKey { get; set; }    
        public string ChannelName { get; set; } = string.Empty;
        public string ChannelType { get; set; } = string.Empty;
    }
}
