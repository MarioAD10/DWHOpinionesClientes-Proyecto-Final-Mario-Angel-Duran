using Core.Application.DTO.APIDto;
using Core.Application.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Extractors.Extractors.API
{
    public class ApiSocialCommentsExtractor : IExtractorGeneric<SocialCommentDto>
    {
        private readonly string _apiUrl;
        private readonly string _dwConnectionString; 
        private readonly ILogger<ApiSocialCommentsExtractor> _logger;
        private readonly HttpClient _httpClient;

        public string SourceName => "SocialCommentsAPI";

        public ApiSocialCommentsExtractor(
            string apiUrl,
            string dwConnectionString, 
            ILogger<ApiSocialCommentsExtractor> logger)
        {
            _apiUrl = apiUrl;
            _dwConnectionString = dwConnectionString; 
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable<SocialCommentDto>> ExtractAsync()
        {
            var comments = new List<SocialCommentDto>();

            try
            {
                _logger.LogInformation($"Solicitando datos desde API: {_apiUrl}");

                // ============================================
                // PASO 1: EXTRAER DATOS DE LA API
                // ============================================
                var response = await _httpClient.GetAsync(_apiUrl);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                comments = JsonSerializer.Deserialize<List<SocialCommentDto>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<SocialCommentDto>();

                _logger.LogInformation($"{comments.Count} registros extraídos desde el API REST.");

                // ============================================
                // PASO 2: ENRIQUECER CON TABLAS MAESTRAS
                // ============================================
                _logger.LogInformation($"Enriqueciendo datos con tablas maestras...");

                comments = await EnrichRecordsAsync(comments);

                _logger.LogInformation($"Extracción completada: {comments.Count} registros enriquecidos.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error al conectar con la API: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error durante la extracción desde API: {ex.Message}");
            }

            return comments;
        }

        /// <summary>
        /// Enriquece los registros con datos de las tablas maestras.
        /// </summary>
        private async Task<List<SocialCommentDto>> EnrichRecordsAsync(List<SocialCommentDto> rawRecords)
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

        /// <summary>
        /// Obtiene todos los clientes usando el Stored Procedure.
        /// </summary>
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

        /// <summary>
        /// Obtiene todos los productos usando el Stored Procedure.
        /// </summary>
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
                _logger.LogError($"Error al obtener productos via SP: {ex.Message}");
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
