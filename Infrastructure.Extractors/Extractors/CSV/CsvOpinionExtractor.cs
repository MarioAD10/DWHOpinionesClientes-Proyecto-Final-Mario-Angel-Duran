using Core.Application.DTO.CSVDto;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Core.Application.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Extractors.Extractors.CSV
{
    /// <summary>
    /// Extractor que lee el archivo surveys_part1.csv y enriquece los datos con tablas maestras usando Stored Procedures.
    /// </summary>
    public class CsvOpinionExtractor : IExtractorGeneric<FactOpinionCsvDto>
    {
        private readonly string _filePath;
        private readonly ILogger<CsvOpinionExtractor> _logger;
        private readonly string _connectionString;

        public CsvOpinionExtractor(
            string filePath,
            ILogger<CsvOpinionExtractor> logger,
            string connectionString)
        {
            _filePath = filePath;
            _logger = logger;
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<FactOpinionCsvDto>> ExtractAsync()
        {
            var records = new List<FactOpinionCsvDto>();

            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogError($"No se encontró el archivo CSV en la ruta: {_filePath}");
                    return records;
                }

                _logger.LogInformation($"Iniciando extracción desde {_filePath} ...");

                // ============================================
                // PASO 1: LEER EL CSV
                // ============================================
                using var reader = new StreamReader(_filePath, System.Text.Encoding.UTF8);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    Delimiter = ","
                });

                var rawRecords = csv.GetRecords<FactOpinionCsvDto>().ToList();

                _logger.LogInformation($"{rawRecords.Count} registros leídos del CSV");

                // ============================================
                // PASO 2: ENRIQUECER CON DATOS DE MAESTROS USANDO STORED PROCEDURES
                // ============================================
                _logger.LogInformation($"Enriqueciendo datos con tablas maestras (via Stored Procedures)...");

                records = await EnrichRecordsAsync(rawRecords);

                _logger.LogInformation($"Extracción completada: {records.Count} registros enriquecidos.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al procesar el archivo CSV: {ex.Message}");
            }

            return records;
        }

        /// <summary>
        /// Enriquece los registros del CSV con datos de las tablas maestras usando Stored Procedures.
        /// </summary>
        private async Task<List<FactOpinionCsvDto>> EnrichRecordsAsync(List<FactOpinionCsvDto> rawRecords)
        {
            var enrichedRecords = new List<FactOpinionCsvDto>();

            try
            {
                // ============================================
                // OBTENER DATOS DE MAESTROS USANDO STORED PROCEDURES
                // ============================================
                var clientes = await GetClientesViaSPAsync();
                var productos = await GetProductosViaSPAsync();

                foreach (var record in rawRecords)
                {
                    if (record.IdOpinion == 0 || record.IdCliente == 0 || record.IdProducto == 0)
                        continue;

                    if (clientes.TryGetValue(record.IdCliente, out var cliente))
                    {
                        record.NombreCliente = cliente.Nombre;
                        record.Genero = cliente.Genero;
                        record.RangoEdad = cliente.RangoEdad;
                        record.Pais = cliente.Pais;
                    }
                    else
                    {
                        _logger.LogWarning($"Cliente {record.IdCliente} no encontrado en Maestro_Clientes");
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
                        _logger.LogWarning($"Producto {record.IdProducto} no encontrado en Maestro_Productos");
                        record.NombreProducto = $"Producto {record.IdProducto}";
                        record.Marca = "Marca Genérica";
                        record.Categoria = "Categoría General";
                        record.Precio = 0;
                    }

                    enrichedRecords.Add(record);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enriquecer registros: {ex.Message}");
            }

            return enrichedRecords;
        }

        /// <summary>
        /// Obtiene todos los clientes usando el Stored Procedure sp_GetMaestroClientes.
        /// </summary>
        private async Task<Dictionary<int, (string Nombre, string Genero, string RangoEdad, string Pais)>> GetClientesViaSPAsync()
        {
            var clientes = new Dictionary<int, (string, string, string, string)>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
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
        /// Obtiene todos los productos usando el Stored Procedure sp_GetMaestroProductos.
        /// </summary>
        private async Task<Dictionary<int, (string Nombre, string Marca, string Categoria, decimal Precio)>> GetProductosViaSPAsync()
        {
            var productos = new Dictionary<int, (string, string, string, decimal)>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
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
    // DTOs INTERNOS PARA MAPEAR RESULTADOS DE LOS SPs
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
