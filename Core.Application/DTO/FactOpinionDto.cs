using System.ComponentModel.DataAnnotations;

namespace Core.Application.DTO
{
    public class FactOpinionDto
    {
        public int CustomerKey { get; set; }
        public int DateKey { get; set; }
        public int ProductKey { get; set; }
        public int SourceKey { get; set; }
        public int SentimentKey { get; set; }
        public int ChannelKey { get; set; }
        public int ETLBatchKey { get; set; }
        public decimal SentimentScore { get; set; }
        public decimal SatisfactionScore { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }
}
