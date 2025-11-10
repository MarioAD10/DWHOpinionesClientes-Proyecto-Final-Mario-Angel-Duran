namespace Core.Application.Models
{
    public class ExtractionParams
    {
        public string SourcePath { get; set; } = string.Empty;
        public string SourceType {  get; set; } = string.Empty;
        public string? ConnectionString { get; set; }
        public string? ApiEndpoint { get; set; }
        public DateTime? LastExtractionDate { get; set; }   
        public string? DestinationTable { get; set; }          

    }
}
