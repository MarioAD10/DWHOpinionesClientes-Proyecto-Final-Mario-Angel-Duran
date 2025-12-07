using Core.Application.DTO.CSVDto;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Fact;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la tabla de hechos Fact_SurveyResponse.
    /// </summary>
    public class FactSurveyResponseLoader : BaseDimensionLoader, IFactSurveyResponseLoader
    {
        public FactSurveyResponseLoader(
            DWOpinionesContext context,
            ILogger<FactSurveyResponseLoader> logger)
            : base(context, logger)
        {
        }

        public async Task LoadSurveyResponsesAsync(IEnumerable<FactOpinionCsvDto> csvRecords)
        {
            _logger.LogInformation("Iniciando carga de Fact_SurveyResponse...");

            await ClearFactSurveyResponseAsync();

            int totalInserted = 0;

            // ============================================
            // CARGAR DESDE CSV 
            // ============================================
            _logger.LogInformation($"Procesando {csvRecords.Count()} registros del CSV...");
            totalInserted += await LoadFromCsvAsync(csvRecords);

            _logger.LogInformation($"Fact_SurveyResponse cargada: {totalInserted} registros insertados.");
        }

        private async Task ClearFactSurveyResponseAsync()
        {
            _logger.LogInformation("Limpiando Fact_SurveyResponse...");

            var existingRecords = await _context.FactSurveyResponse.ToListAsync();

            if (existingRecords.Any())
            {
                _context.FactSurveyResponse.RemoveRange(existingRecords);
                await SaveChangesAsync();
                _logger.LogInformation($"{existingRecords.Count} registros eliminados de Fact_SurveyResponse");
            }
            else
            {
                _logger.LogInformation("Fact_SurveyResponse ya está vacía");
            }
        }

        private async Task<int> LoadFromCsvAsync(IEnumerable<FactOpinionCsvDto> csvRecords)
        {
            int insertedCount = 0;
            int skippedCount = 0;

            var allQuestions = await _context.DimSurveyQuestion.ToListAsync();

            if (!allQuestions.Any())
            {
                _logger.LogError("No hay preguntas de encuesta disponibles en Dim_SurveyQuestion");
                return 0;
            }

            var random = new Random();

            foreach (var record in csvRecords)
            {
                try
                {
                    var customerKey = await GetCustomerKeyAsync(record.NombreCliente);
                    var productKey = await GetProductKeyAsync(record.NombreProducto, record.Marca, record.Categoria);
                    var dateKey = await GetDateKeyAsync(record.Fecha);
                    var sourceKey = await GetSourceKeyAsync("Encuestas CSV");
                    var channelKey = await GetChannelKeyAsync("Encuesta Online");

                    var randomQuestion = allQuestions[random.Next(allQuestions.Count)];
                    var surveyQuestionKey = randomQuestion.SurveyQuestionKey;

                    if (customerKey == -1 || productKey == -1 || dateKey == -1 ||
                        sourceKey == -1 || channelKey == -1)
                    {
                        _logger.LogWarning($"Registro {record.IdOpinion} omitido por claves inválidas");
                        skippedCount++;
                        continue;
                    }

                    bool isValidResponse = record.PuntajeSatisfacción >= 1 && record.PuntajeSatisfacción <= 5;
                    int responseTimeSec = random.Next(10, 301);

                    var factSurveyResponse = new FactSurveyResponseRecord
                    {
                        CustomerKey = customerKey,
                        ProductKey = productKey,
                        DateKey = dateKey,
                        SourceKey = sourceKey,
                        ChannelKey = channelKey,
                        SurveyQuestionKey = surveyQuestionKey, 
                        ETLBatchKey = 1,
                        ResponseValue = record.PuntajeSatisfacción,
                        ResponseTimeSec = responseTimeSec,
                        IsValidResponse = isValidResponse,
                        IngestionTimestamp = DateTime.Now
                    };

                    await _context.FactSurveyResponse.AddAsync(factSurveyResponse);
                    insertedCount++;

                    if (insertedCount % 50 == 0)
                    {
                        await SaveChangesAsync();
                        _logger.LogInformation($"{insertedCount} registros guardados del CSV...");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar registro CSV {record.IdOpinion}: {ex.Message}");
                    skippedCount++;
                }
            }

            await SaveChangesAsync();
            _logger.LogInformation($"Total CSV: {insertedCount} insertados, {skippedCount} omitidos");
            return insertedCount;
        }

        // ==================
        // MÉTODOS AUXILIARES 
        // ==================

        private async Task<int> GetCustomerKeyAsync(string customerName)
        {
            var customer = await _context.DimCustomer
                .FirstOrDefaultAsync(c => c.CustomerName == customerName);

            if (customer == null)
            {
                _logger.LogWarning($"Cliente no encontrado: {customerName}");
                return -1;
            }

            return customer.CustomerKey;
        }

        private async Task<int> GetProductKeyAsync(string productName, string brand, string category)
        {
            var product = await _context.DimProduct
                .FirstOrDefaultAsync(p =>
                    p.ProductName == productName &&
                    p.Brand == brand &&
                    p.Category == category);

            if (product == null)
            {
                _logger.LogWarning($"Producto no encontrado: {productName}");
                return -1;
            }

            return product.ProductKey;
        }

        private async Task<int> GetDateKeyAsync(DateTime date)
        {
            var dateKey = int.Parse(date.ToString("yyyyMMdd"));

            var dateRecord = await _context.DimDate
                .FirstOrDefaultAsync(d => d.DateKey == dateKey);

            if (dateRecord == null)
            {
                _logger.LogWarning($"Fecha no encontrada: {date:yyyy-MM-dd}");
                return -1;
            }

            return dateKey;
        }

        private async Task<int> GetSourceKeyAsync(string sourceName)
        {
            var source = await _context.DimSource
                .FirstOrDefaultAsync(s => s.SourceName == sourceName);

            if (source == null)
            {
                _logger.LogWarning($"Fuente no encontrada: {sourceName}");
                return -1;
            }

            return source.SourceKey;
        }

        private async Task<int> GetChannelKeyAsync(string channelName)
        {
            var channel = await _context.DimChannel
                .FirstOrDefaultAsync(c => c.ChannelName == channelName);

            if (channel == null)
            {
                _logger.LogWarning($"Canal no encontrado: {channelName}");
                return -1;
            }

            return channel.ChannelKey;
        }

        private async Task<int> GetSurveyQuestionKeyAsync(string questionText)
        {
            var question = await _context.DimSurveyQuestion
                .FirstOrDefaultAsync(q => q.QuestionText == questionText);

            if (question == null)
            {
                _logger.LogWarning($"Pregunta de encuesta no encontrada: {questionText}");
                return -1;
            }

            return question.SurveyQuestionKey;
        }
    }
}
