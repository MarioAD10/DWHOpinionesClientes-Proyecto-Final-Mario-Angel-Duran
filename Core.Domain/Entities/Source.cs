namespace Core.Domain.Entities
{
    internal class Source
    {
        public int SourceKey { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime LoadDate { get; set; }
    }
}
