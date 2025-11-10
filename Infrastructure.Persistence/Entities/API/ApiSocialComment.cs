namespace Infrastructure.Persistence.Entities.API
{
    public class ApiSocialComment
    {
        public string PostId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public int Likes { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
