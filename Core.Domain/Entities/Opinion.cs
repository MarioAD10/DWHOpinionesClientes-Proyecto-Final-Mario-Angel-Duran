namespace Core.Domain.Entities
{
    public class Opinion
    {
        public int OpinionKey { get; set; }
        public int CustomerKey { get; set; }
        public int ProductKey { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public decimal SatisfactionScore { get; set; }
    }
}
