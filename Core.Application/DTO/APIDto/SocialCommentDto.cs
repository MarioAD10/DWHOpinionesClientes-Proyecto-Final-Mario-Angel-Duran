namespace Core.Application.DTO.APIDto
{
    public class SocialCommentDto
    {
        public int IdComentario { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public int Likes { get; set; }
        public int Shares { get; set; }
        public string Plataforma { get; set; } = string.Empty;
    }
}
