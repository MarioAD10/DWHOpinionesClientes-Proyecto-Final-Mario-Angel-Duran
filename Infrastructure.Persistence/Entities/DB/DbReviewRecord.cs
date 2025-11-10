namespace Infrastructure.Persistence.Entities.DB
{
    public class DbReviewRecord
    {
        public int IdReseña { get; set; }
        public int IdOpinion { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public string Clasificacion { get; set; } = string.Empty;
        public int Puntaje { get; set; }
        public string Fuente { get; set; } = "Sitio Web";
    }
}
