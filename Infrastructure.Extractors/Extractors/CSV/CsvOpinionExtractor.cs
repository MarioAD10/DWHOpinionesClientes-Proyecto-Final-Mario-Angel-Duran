using Core.Application.DTO.CSVDto;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Core.Application.Interfaces;

namespace Infrastructure.Extractors.Extractors.CSV
{
    /// <summary>
    /// Extractor que lee el archivo surveys_part1.csv y convierte cada registro a un FactOpinionDto.
    /// </summary>
    public class CsvOpinionExtractor : IExtractorGeneric<FactOpinionCsvDto>
    {
        private readonly string _filePath;
        private readonly ILogger<CsvOpinionExtractor> _logger;

        public CsvOpinionExtractor(string filePath, ILogger<CsvOpinionExtractor> logger)
        {
            _filePath = filePath;
            _logger = logger;
        }

        public async Task<IEnumerable<FactOpinionCsvDto>> ExtractAsync()
        {
            var records = new List<FactOpinionCsvDto>();

            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogError($"❌ No se encontró el archivo CSV en la ruta: {_filePath}");
                    return records;
                }

                _logger.LogInformation($"📂 Iniciando extracción desde {_filePath} ...");

                using var reader = new StreamReader(_filePath, System.Text.Encoding.UTF8);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    Delimiter = ","
                });

                var rawRecords = csv.GetRecords<FactOpinionCsvDto>();

                foreach (var record in rawRecords)
                {
                    // Validación simple: asegurar que los campos clave no estén vacíos
                    if (record.IdOpinion == 0 || record.IdCliente == 0 || record.IdProducto == 0)
                        continue;

                    records.Add(record);
                }

                _logger.LogInformation($"✅ Extracción completada: {records.Count} registros válidos leídos desde el archivo CSV.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al procesar el archivo CSV: {ex.Message}");
            }

            return await Task.FromResult(records);
        }
    }
}
