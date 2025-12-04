using Core.Application.DTO.DBDto;
using Core.Application.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Extractors.Extractors.DB
{
    public class SqlWebReviewExtractor : IExtractorGeneric<WebReviewDto>
    {
        private readonly string _connectionString;
        private readonly string _dwConnectionString; 
        private readonly ILogger<SqlWebReviewExtractor> _logger;

        public string SourceName => "DBWebReviews";

        public SqlWebReviewExtractor(
            string connectionString,
            string dwConnectionString, 
            ILogger<SqlWebReviewExtractor> logger)
        {
            _connectionString = connectionString;
            _dwConnectionString = dwConnectionString; 
            _logger = logger;
        }

        public async Task<IEnumerable<WebReviewDto>> ExtractAsync()
        {
            var allReviews = new List<WebReviewDto>();
            int batchSize = 1000;
            int offset = 0;
            bool hasMore = true;

            _logger.LogInformation("Iniciando extracción escalable desde DBWebReviews...");

            try
            {
                // ============================================
                // PASO 1: EXTRAER DATOS DE DBWebReview
                // ============================================
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                while (hasMore)
                {
                    var batch = await connection.QueryAsync<WebReviewDto>(
                        "sp_GetWebReviewsBatch",
                        new { BatchSize = batchSize, Offset = offset },
                        commandType: System.Data.CommandType.StoredProcedure);

                    var list = batch.ToList();
                    allReviews.AddRange(list);

                    _logger.LogInformation($"Lote {offset / batchSize + 1}: {list.Count} registros extraídos.");

                    if (list.Count < batchSize)
                        hasMore = false;
                    else
                        offset += batchSize;
                }

                _logger.LogInformation($"{allReviews.Count} registros extraídos de DBWebReview");

                // ============================================
                // PASO 2: ENRIQUECER CON TABLAS MAESTRAS
                // ============================================
                _logger.LogInformation($"Enriqueciendo datos con tablas maestras (via Stored Procedures)...");

                allReviews = await EnrichRecordsAsync(allReviews);

                _logger.LogInformation($"Extracción completada: {allReviews.Count} registros enriquecidos.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error durante la extracción desde DBWebReviews: {ex.Message}");
            }

            return allReviews;
        }

        private async Task<List<WebReviewDto>> EnrichRecordsAsync(List<WebReviewDto> rawRecords)
        {
            try
            {
                var clientes = await GetClientesViaSPAsync();
                var productos = await GetProductosViaSPAsync();

                foreach (var record in rawRecords)
                {
                    if (clientes.TryGetValue(record.IdCliente, out var cliente))
                    {
                        record.NombreCliente = cliente.Nombre;
                        record.Genero = cliente.Genero;
                        record.RangoEdad = cliente.RangoEdad;
                        record.Pais = cliente.Pais;
                    }
                    else
                    {
                        record.NombreCliente = $"Cliente {record.IdCliente}";
                        record.Genero = "Desconocido";
                        record.RangoEdad = "Desconocido";
                        record.Pais = "Desconocido";
                    }

                    if (productos.TryGetValue(record.IdProducto, out var producto))
                    {
                        record.NombreProducto = producto.Nombre;
                        record.Marca = producto.Marca;
                        record.Categoria = producto.Categoria;
                        record.Precio = producto.Precio;
                    }
                    else
                    {
                        record.NombreProducto = $"Producto {record.IdProducto}";
                        record.Marca = "Marca Genérica";
                        record.Categoria = "Categoría General";
                        record.Precio = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enriquecer registros: {ex.Message}");
            }

            return rawRecords;
        }

        private async Task<Dictionary<int, (string Nombre, string Genero, string RangoEdad, string Pais)>> GetClientesViaSPAsync()
        {
            var clientes = new Dictionary<int, (string, string, string, string)>();

            try
            {
                using var connection = new SqlConnection(_dwConnectionString);
                await connection.OpenAsync();

                var result = await connection.QueryAsync<MaestroClienteDto>(
                    "sp_GetMaestroClientes",
                    commandType: System.Data.CommandType.StoredProcedure);

                foreach (var cliente in result)
                {
                    clientes[cliente.IdCliente] = (
                        cliente.NombreCliente,
                        cliente.Genero,
                        cliente.RangoEdad,
                        cliente.Pais
                    );
                }

                _logger.LogInformation($"{clientes.Count} clientes cargados desde Maestro_Clientes (via SP)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener clientes via SP: {ex.Message}");
            }

            return clientes;
        }

        private async Task<Dictionary<int, (string Nombre, string Marca, string Categoria, decimal Precio)>> GetProductosViaSPAsync()
        {
            var productos = new Dictionary<int, (string, string, string, decimal)>();

            try
            {
                using var connection = new SqlConnection(_dwConnectionString);
                await connection.OpenAsync();

                var result = await connection.QueryAsync<MaestroProductoDto>(
                    "sp_GetMaestroProductos",
                    commandType: System.Data.CommandType.StoredProcedure);

                foreach (var producto in result)
                {
                    productos[producto.IdProducto] = (
                        producto.NombreProducto,
                        producto.Marca,
                        producto.Categoria,
                        producto.Precio
                    );
                }

                _logger.LogInformation($"{productos.Count} productos cargados desde Maestro_Productos (via SP)");
            }
            catch (Exception ex)
            {
                _logger.LogError($" Error al obtener productos via SP: {ex.Message}");
            }

            return productos;
        }
    }

    // ============================================
    // DTOs INTERNOS
    // ============================================
    internal class MaestroClienteDto
    {
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public string RangoEdad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
    }

    internal class MaestroProductoDto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
    }
}
