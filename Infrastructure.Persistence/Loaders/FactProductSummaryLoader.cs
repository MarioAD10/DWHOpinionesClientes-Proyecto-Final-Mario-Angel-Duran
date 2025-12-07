using Core.Application.DTO.APIDto;
using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Fact;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la tabla de hechos Fact_ProductSummary.
    /// </summary>
    public class FactProductSummaryLoader : BaseDimensionLoader, IFactProductSummaryLoader
    {
        public FactProductSummaryLoader(
            DWOpinionesContext context,
            ILogger<FactProductSummaryLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga resumen de productos agregando métricas de todas las fuentes.
        /// </summary>
        public async Task LoadProductSummaryAsync(
            IEnumerable<FactOpinionCsvDto> csvRecords,
            IEnumerable<WebReviewDto> dbRecords,
            IEnumerable<SocialCommentDto> apiRecords)
        {
            _logger.LogInformation("Iniciando carga de Fact_ProductSummary...");

            await ClearFactProductSummaryAsync();

            int totalInserted = 0;

            // Combinar todas las fuentes de datos
            var allOpinions = new List<OpinionSummaryData>();

            // Procesar CSV
            foreach (var record in csvRecords)
            {
                allOpinions.Add(new OpinionSummaryData
                {
                    ProductName = record.NombreProducto,
                    Brand = record.Marca,
                    Category = record.Categoria,
                    Date = record.Fecha,
                    SourceName = "Encuestas CSV",
                    ChannelName = "Encuesta Online",
                    SentimentScore = MapSentimentToScore(record.Clasificación),
                    SentimentCategory = MapSentimentCategory(record.Clasificación),
                    SatisfactionScore = record.PuntajeSatisfacción
                });
            }

            // Procesar DB
            foreach (var record in dbRecords)
            {
                allOpinions.Add(new OpinionSummaryData
                {
                    ProductName = record.NombreProducto,
                    Brand = record.Marca,
                    Category = record.Categoria,
                    Date = record.Fecha,
                    SourceName = "Reseñas Web",
                    ChannelName = "Web",
                    SentimentScore = MapSentimentToScore(record.Clasificacion),
                    SentimentCategory = MapSentimentCategory(record.Clasificacion),
                    SatisfactionScore = record.Puntaje
                });
            }

            // Procesar API
            foreach (var record in apiRecords)
            {
                allOpinions.Add(new OpinionSummaryData
                {
                    ProductName = record.NombreProducto,
                    Brand = record.Marca,
                    Category = record.Categoria,
                    Date = record.Fecha,
                    SourceName = "Redes Sociales API",
                    ChannelName = record.Plataforma,
                    SentimentScore = 0,
                    SentimentCategory = "Neutro",
                    SatisfactionScore = 3
                });
            }

            // CAMBIO CLAVE: Agrupar SIN SentimentCategory
            var groupedData = allOpinions
                .GroupBy(o => new
                {
                    o.ProductName,
                    o.Brand,
                    o.Category,
                    Date = o.Date.Date,
                    o.SourceName,
                    o.ChannelName
                    // ❌ NO incluir SentimentCategory aquí
                });

            _logger.LogInformation($"Procesando {groupedData.Count()} agrupaciones de productos...");

            foreach (var group in groupedData)
            {
                try
                {
                    var productKey = await GetProductKeyAsync(group.Key.ProductName, group.Key.Brand, group.Key.Category);
                    var dateKey = await GetDateKeyAsync(group.Key.Date);
                    var sourceKey = await GetSourceKeyAsync(group.Key.SourceName);
                    var channelKey = await GetChannelKeyAsync(group.Key.ChannelName);

                    if (productKey == -1 || dateKey == -1 || sourceKey == -1 || channelKey == -1)
                    {
                        _logger.LogWarning($"Agrupación omitida por claves inválidas: {group.Key.ProductName}");
                        continue;
                    }

                    // Calcular métricas agregadas del grupo completo
                    var opinions = group.ToList();
                    int totalOpinions = opinions.Count;
                    decimal avgSentimentScore = opinions.Average(o => o.SentimentScore);
                    decimal avgSatisfaction = opinions.Average(o => o.SatisfactionScore);

                    // Calcular porcentajes por sentimiento del GRUPO COMPLETO
                    int positiveCount = opinions.Count(o => o.SentimentScore > 0);
                    int negativeCount = opinions.Count(o => o.SentimentScore < 0);
                    int neutralCount = opinions.Count(o => o.SentimentScore == 0);

                    decimal positivePercent = totalOpinions > 0 ? Math.Round((decimal)positiveCount / totalOpinions * 100, 2) : 0;
                    decimal negativePercent = totalOpinions > 0 ? Math.Round((decimal)negativeCount / totalOpinions * 100, 2) : 0;
                    decimal neutralPercent = totalOpinions > 0 ? Math.Round((decimal)neutralCount / totalOpinions * 100, 2) : 0;

                    // Determinar el sentimiento predominante del grupo
                    string predominantSentiment = "Neutro";
                    if (positiveCount > negativeCount && positiveCount > neutralCount)
                        predominantSentiment = "Positivo";
                    else if (negativeCount > positiveCount && negativeCount > neutralCount)
                        predominantSentiment = "Negativo";

                    var sentimentKey = await GetSentimentKeyAsync(predominantSentiment);

                    if (sentimentKey == -1)
                    {
                        _logger.LogWarning($"Sentimiento no encontrado: {predominantSentiment}");
                        continue;
                    }

                    // Crear UN SOLO registro de resumen por grupo
                    var productSummary = new FactProductSummaryRecord
                    {
                        ProductKey = productKey,
                        DateKey = dateKey,
                        SourceKey = sourceKey,
                        ChannelKey = channelKey,
                        SentimentKey = sentimentKey, // Sentimiento predominante
                        ETLBatchKey = 1,
                        TotalOpinions = totalOpinions,
                        AvgSentimentScore = Math.Round(avgSentimentScore, 2),
                        AvgSatisfaction = Math.Round(avgSatisfaction, 2),
                        PositivePercent = positivePercent,
                        NegativePercent = negativePercent,
                        NeutralPercent = neutralPercent
                    };

                    await _context.FactProductSummary.AddAsync(productSummary);
                    totalInserted++;

                    if (totalInserted % 50 == 0)
                    {
                        await SaveChangesAsync();
                        _logger.LogInformation($"{totalInserted} resúmenes de productos guardados...");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar resumen de producto {group.Key.ProductName}: {ex.Message}");
                }
            }

            await SaveChangesAsync();
            _logger.LogInformation($"Fact_ProductSummary cargada: {totalInserted} registros insertados.");
        }

        private async Task ClearFactProductSummaryAsync()
        {
            _logger.LogInformation("Limpiando Fact_ProductSummary...");

            var existingRecords = await _context.FactProductSummary.ToListAsync();

            if (existingRecords.Any())
            {
                _context.FactProductSummary.RemoveRange(existingRecords);
                await SaveChangesAsync();
                _logger.LogInformation($"{existingRecords.Count} registros eliminados de Fact_ProductSummary");
            }
            else
            {
                _logger.LogInformation("Fact_ProductSummary ya está vacía");
            }
        }

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================

        /// <summary>
        /// Mapea la clasificación textual a un score numérico.
        /// </summary>
        private decimal MapSentimentToScore(string clasificacion)
        {
            return clasificacion?.ToLower() switch
            {
                "positiva" or "positive" => 1.0m,
                "neutra" or "neutral" => 0.0m,
                "negativa" or "negative" => -1.0m,
                _ => 0.0m
            };
        }

        /// <summary>
        /// Mapea la clasificación textual a categoría para el key.
        /// </summary>
        private string MapSentimentCategory(string clasificacion)
        {
            return clasificacion?.ToLower() switch
            {
                "positiva" or "positive" => "Positivo",
                "neutra" or "neutral" => "Neutro",
                "negativa" or "negative" => "Negativo",
                _ => "Neutro"
            };
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

        private async Task<int> GetSentimentKeyAsync(string sentimentName)
        {
            var sentiment = await _context.DimSentiment
                .FirstOrDefaultAsync(s => s.SentimentName == sentimentName);

            if (sentiment == null)
            {
                _logger.LogWarning($"Sentimiento no encontrado: {sentimentName}");
                return -1;
            }

            return sentiment.SentimentKey;
        }

        // Clase auxiliar interna
        private class OpinionSummaryData
        {
            public string ProductName { get; set; } = string.Empty;
            public string Brand { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string SourceName { get; set; } = string.Empty;
            public string ChannelName { get; set; } = string.Empty;
            public decimal SentimentScore { get; set; }
            public string SentimentCategory { get; set; } = string.Empty;
            public decimal SatisfactionScore { get; set; }
        }
    }
}