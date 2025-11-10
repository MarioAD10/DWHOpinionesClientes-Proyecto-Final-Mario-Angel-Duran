namespace Core.Domain.Entities
{
    public class ETLBatch
    {
        public int BatchKey { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }
    }
}
