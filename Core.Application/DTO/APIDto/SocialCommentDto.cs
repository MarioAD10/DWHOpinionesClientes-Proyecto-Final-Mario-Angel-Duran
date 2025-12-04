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


        public string NombreCliente { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public string RangoEdad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;

        public string NombreProducto { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
    }
}
