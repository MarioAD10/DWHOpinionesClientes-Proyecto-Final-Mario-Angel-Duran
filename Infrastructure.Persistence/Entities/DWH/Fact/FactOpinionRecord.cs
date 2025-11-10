namespace Infrastructure.Persistence.Entities.DWH.Fact
{
    public class FactOpinionRecord
    {
        public int OpinionKey { get; set; }                 
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
        public DateTime IngestionTimestamp { get; set; }
    }
}

