namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión de fuentes de datos (sitio web, encuesta, API, etc.).
    /// </summary>
    public class DimSourceRecord
    {
        public int SourceKey { get; set; }                 
        public string SourceName { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
    }
}
