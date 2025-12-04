using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Entities.DWH.Fact
{
    public class FactEngagementRecord
    {
        [Key]
        public int EngagementKey { get; set; }              
        public int CustomerKey { get; set; }           
        public int DateKey { get; set; }            
        public int ProductKey { get; set; }                
        public int SourceKey { get; set; }                 
        public int ChannelKey { get; set; }                
        public int ETLBatchKey { get; set; }             

        public int LikesCount { get; set; }
        public int SharesCount { get; set; }
        public int CommentsCount { get; set; }
        public int ViewsCount { get; set; }
        public int RepliesCount { get; set; }
        public decimal EngagementRate { get; set; }
        public DateTime IngestionTimestamp { get; set; }
    }
}
