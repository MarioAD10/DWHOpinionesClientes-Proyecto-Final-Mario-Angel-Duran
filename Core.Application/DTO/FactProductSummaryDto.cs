namespace Core.Application.DTO
{
    public class FactProductSummaryDto
    {
        public int DateKey { get; set; }
        public int ProductKey { get; set; }
        public int SourceKey { get; set; }
        public int ChannelKey { get; set; }
        public int ETLBatchKey { get; set; }
        public int SentimentKey { get; set; }

        public int TotalOpinions { get; set; }
        public decimal AvgSentimentScore { get; set; }
        public decimal AvgSatisfaction { get; set; }
        public decimal PositivePercent { get; set; }
        public decimal NegativePercent { get; set; }
        public decimal NeutralPercent { get; set; }
    }
}
