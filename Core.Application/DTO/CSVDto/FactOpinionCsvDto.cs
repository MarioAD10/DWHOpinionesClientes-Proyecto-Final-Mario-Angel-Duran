namespace Core.Application.DTO.CSVDto
{
    public class FactOpinionCsvDto
    {
        public int IdOpinion { get; set; }
        public int IdCliente { get; set; }
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public string Clasificación { get; set; } = string.Empty;
        public decimal PuntajeSatisfacción { get; set; }
        public string Fuente { get; set; } = string.Empty;


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
