using System.ComponentModel.DataAnnotations;

namespace Core.Application.DTO
{
    public class FactEngagementDto
    {
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
    }
}
