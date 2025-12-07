using Core.Application.DTO.APIDto;
using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Infrastructure.Persistence.Entities.DWH.Fact;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la tabla de hechos Fact_Opinion.
    /// </summary>
    public class FactOpinionLoader : BaseDimensionLoader, IFactOpinionLoader
    {
        public FactOpinionLoader(
            DWOpinionesContext context,
            ILogger<FactOpinionLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga opiniones desde las 3 fuentes de datos.
        /// </summary>
        public async Task LoadOpinionsAsync(
            IEnumerable<FactOpinionCsvDto> csvRecords,
            IEnumerable<WebReviewDto> dbRecords,
            IEnumerable<SocialCommentDto> apiRecords)
        {
            _logger.LogInformation("Iniciando carga de Fact_Opinion...");

            await ClearFactOpinionAsync();

            // ============================================
            // ASEGURAR QUE TODAS LAS FECHAS EXISTAN
            // ============================================
            await EnsureDatesExistAsync(csvRecords, dbRecords, apiRecords);


            int totalInserted = 0;

            // ============================================
            // CARGAR DESDE CSV
            // ============================================
            _logger.LogInformation($"Procesando {csvRecords.Count()} registros del CSV...");
            totalInserted += await LoadFromCsvAsync(csvRecords);

            // ============================================
            // CARGAR DESDE BASE DE DATOS
            // ============================================
            _logger.LogInformation($"Procesando {dbRecords.Count()} registros de DBWebReview...");
            totalInserted += await LoadFromDbAsync(dbRecords);

            // ============================================
            // CARGAR DESDE API
            // ============================================
            _logger.LogInformation($"Procesando {apiRecords.Count()} registros de la API...");
            totalInserted += await LoadFromApiAsync(apiRecords);

            _logger.LogInformation($"Fact_Opinion cargada: {totalInserted} registros insertados.");
        }

        private async Task ClearFactOpinionAsync()
        {
            _logger.LogInformation("Limpiando Fact_Opinion...");

            var existingRecords = await _context.FactOpinion.ToListAsync();

            if (existingRecords.Any())
            {
                _context.FactOpinion.RemoveRange(existingRecords);
                await SaveChangesAsync();
                _logger.LogInformation($"{existingRecords.Count} registros eliminados de Fact_Opinion");
            }
            else
            {
                _logger.LogInformation("Fact_Opinion ya está vacía");
            }
        }

        /// <summary>
        /// Carga opiniones desde CSV.
        /// </summary>
        private async Task<int> LoadFromCsvAsync(IEnumerable<FactOpinionCsvDto> csvRecords)
        {
            int insertedCount = 0;

            foreach (var record in csvRecords)
            {
                try
                {
                    // Obtener las Foreign Keys de las dimensiones
                    var customerKey = await GetCustomerKeyAsync(record.NombreCliente);
                    var productKey = await GetProductKeyAsync(record.NombreProducto, record.Marca, record.Categoria);
                    var dateKey = await GetDateKeyAsync(record.Fecha);
                    var sourceKey = await GetSourceKeyAsync("Encuestas CSV");
                    var channelKey = await GetChannelKeyAsync("Encuesta Online");
                    var sentimentKey = await GetSentimentKeyAsync(record.Clasificación);

                    // Crear el registro de Fact_Opinion
                    var factOpinion = new FactOpinionRecord
                    {
                        CustomerKey = customerKey,
                        ProductKey = productKey,
                        DateKey = dateKey,
                        SourceKey = sourceKey,
                        ChannelKey = channelKey,
                        SentimentKey = sentimentKey,
                        ETLBatchKey = 1, 
                        SentimentScore = MapSentimentToScore(record.Clasificación),
                        SatisfactionScore = record.PuntajeSatisfacción,
                        CommentText = record.Comentario ?? string.Empty,
                        IngestionTimestamp = DateTime.Now
                    };

                    await _context.FactOpinion.AddAsync(factOpinion);
                    insertedCount++;

                    if (insertedCount % 100 == 0)
                    {
                        await SaveChangesAsync();
                        _logger.LogInformation($"{insertedCount} registros guardados del CSV...");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar registro CSV {record.IdOpinion}: {ex.Message}");
                }
            }

            await SaveChangesAsync();
            return insertedCount;
        }

        /// <summary>
        /// Carga opiniones desde la base de datos.
        /// </summary>
        private async Task<int> LoadFromDbAsync(IEnumerable<WebReviewDto> dbRecords)
        {
            int insertedCount = 0;
            int skippedCount = 0;

            foreach (var record in dbRecords)
            {
                try
                {
                    var customerKey = await GetCustomerKeyAsync(record.NombreCliente);
                    var productKey = await GetProductKeyAsync(record.NombreProducto, record.Marca, record.Categoria);
                    var dateKey = await GetDateKeyAsync(record.Fecha);
                    var sourceKey = await GetSourceKeyAsync("Reseñas Web");
                    var channelKey = await GetChannelKeyAsync("Web");
                    var sentimentKey = await GetSentimentKeyAsync(record.Clasificacion);

                    if (customerKey == -1 || productKey == -1 || dateKey == -1 ||
                        sourceKey == -1 || channelKey == -1 || sentimentKey == -1)
                    {
                        _logger.LogWarning($"Registro DB {record.IdReseña} omitido por claves inválidas");
                        skippedCount++;
                        continue;
                    }

                    var factOpinion = new FactOpinionRecord
                    {
                        CustomerKey = customerKey,
                        ProductKey = productKey,
                        DateKey = dateKey,
                        SourceKey = sourceKey,
                        ChannelKey = channelKey,
                        SentimentKey = sentimentKey,
                        ETLBatchKey = 1,
                        SentimentScore = MapSentimentToScore(record.Clasificacion),
                        SatisfactionScore = record.Puntaje,
                        CommentText = record.Comentario ?? string.Empty,
                        IngestionTimestamp = DateTime.Now
                    };

                    await _context.FactOpinion.AddAsync(factOpinion);
                    insertedCount++;

                    if (insertedCount % 50 == 0) 
                    {
                        await SaveChangesAsync();
                        _logger.LogInformation($"{insertedCount} registros guardados, {skippedCount} omitidos");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar registro DB {record.IdReseña}: {ex.Message}");
                    skippedCount++;
                }
            }

            await SaveChangesAsync();
            _logger.LogInformation($" Total DB: {insertedCount} insertados, {skippedCount} omitidos");
            return insertedCount;
        }

        /// <summary>
        /// Carga opiniones desde la API.
        /// </summary>
        private async Task<int> LoadFromApiAsync(IEnumerable<SocialCommentDto> apiRecords)
        {
            int insertedCount = 0;

            foreach (var record in apiRecords)
            {
                try
                {
                    var customerKey = await GetCustomerKeyAsync(record.NombreCliente);
                    var productKey = await GetProductKeyAsync(record.NombreProducto, record.Marca, record.Categoria);
                    var dateKey = await GetDateKeyAsync(record.Fecha);
                    var sourceKey = await GetSourceKeyAsync("Redes Sociales API");
                    var channelKey = await GetChannelKeyAsync(record.Plataforma);
                    var sentimentKey = await GetSentimentKeyAsync("Neutral");

                    var factOpinion = new FactOpinionRecord
                    {
                        CustomerKey = customerKey,
                        ProductKey = productKey,
                        DateKey = dateKey,
                        SourceKey = sourceKey,
                        ChannelKey = channelKey,
                        SentimentKey = sentimentKey,
                        ETLBatchKey = 1,
                        SentimentScore = 0, 
                        SatisfactionScore = 3,
                        CommentText = record.Comentario ?? string.Empty,
                        IngestionTimestamp = DateTime.Now
                    };

                    await _context.FactOpinion.AddAsync(factOpinion);
                    insertedCount++;

                    if (insertedCount % 100 == 0)
                    {
                        await SaveChangesAsync();
                        _logger.LogInformation($"{insertedCount} registros guardados de API...");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar registro API {record.IdComentario}: {ex.Message}");
                }
            }

            await SaveChangesAsync();
            return insertedCount;
        }

        // ============================================
        // MÉTODOS AUXILIARES PARA OBTENER FOREIGN KEYS
        // ============================================

        private async Task<int> GetCustomerKeyAsync(string customerName)
        {
            var customer = await _context.DimCustomer
                .FirstOrDefaultAsync(c => c.CustomerName == customerName);

            if (customer == null)
            {
                _logger.LogWarning($"Cliente no encontrado, creando: {customerName}");

                var newCustomer = new DimCustomerRecord
                {
                    CustomerName = customerName,
                    Gender = "Desconocido",
                    AgeRange = "Desconocido",
                    Country = "Desconocido"
                };

                await _context.DimCustomer.AddAsync(newCustomer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cliente creado con Key: {newCustomer.CustomerKey}");
                return newCustomer.CustomerKey;
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
                _logger.LogWarning($"⚠️ Producto no encontrado, creando: {productName} - {brand}");

                var newProduct = new DimProductRecord
                {
                    ProductName = productName,
                    Brand = brand ?? "Desconocido",
                    Category = category ?? "General",
                    Price = 0,
                    IsActive = true
                };

                await _context.DimProduct.AddAsync(newProduct);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Producto creado con Key: {newProduct.ProductKey}");
                return newProduct.ProductKey;
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
            var mappedSentiment = sentimentName?.Trim().ToLower() switch
            {
                "positiva" => "Positivo",
                "negativa" => "Negativo",
                "neutra" => "Neutro",
                "neutro" => "Neutro",
                "positive" => "Positivo",
                "negative" => "Negativo",
                "neutral" => "Neutro",
                _ => "Neutro" // 
            };

            _logger.LogInformation($"Buscando sentimiento: '{sentimentName}' Mapeado a: '{mappedSentiment}'");

            var sentiment = await _context.DimSentiment
                .FirstOrDefaultAsync(s => s.SentimentName == mappedSentiment);

            if (sentiment == null)
            {
                _logger.LogError($" CRÍTICO: Sentimiento '{mappedSentiment}' no existe en Dim_Sentiment");

                var anySentiment = await _context.DimSentiment.FirstOrDefaultAsync();

                if (anySentiment != null)
                {
                    _logger.LogWarning($" Usando sentimiento por defecto: {anySentiment.SentimentName} (Key: {anySentiment.SentimentKey})");
                    return anySentiment.SentimentKey;
                }

                // Si no hay NINGÚN sentimiento, lanzar excepción
                throw new InvalidOperationException("No hay sentimientos en Dim_Sentiment. Debes cargar la dimensión primero.");
            }

            return sentiment.SentimentKey;
        }

        /// <summary>
        /// Asegura que todas las fechas necesarias estén en la dimensión antes de cargar hechos.
        /// </summary>
        private async Task EnsureDatesExistAsync(
            IEnumerable<FactOpinionCsvDto> csvRecords,
            IEnumerable<WebReviewDto> dbRecords,
            IEnumerable<SocialCommentDto> apiRecords)
        {
            _logger.LogInformation("Verificando fechas faltantes...");

            var allDates = new List<DateTime>();
            allDates.AddRange(csvRecords.Select(r => r.Fecha.Date));
            allDates.AddRange(dbRecords.Select(r => r.Fecha.Date));
            allDates.AddRange(apiRecords.Select(r => r.Fecha.Date));

            var uniqueDates = allDates.Distinct().ToList();

            int addedDates = 0;

            foreach (var date in uniqueDates)
            {
                var dateKey = int.Parse(date.ToString("yyyyMMdd"));

                // Verificar si la fecha ya existe
                var existingDate = await _context.DimDate
                    .FirstOrDefaultAsync(d => d.DateKey == dateKey);

                if (existingDate == null)
                {
                    // Agregar la fecha faltante
                    var newDate = new DimDateRecord
                    {
                        DateKey = dateKey,
                        FullDate = date,
                        Day = (byte)date.Day,
                        Month = (byte)date.Month,
                        MonthName = date.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")),
                        Quarter = (byte)((date.Month - 1) / 3 + 1),
                        Year = date.Year
                    };

                    await _context.DimDate.AddAsync(newDate);
                    addedDates++;
                }
            }

            if (addedDates > 0)
            {
                await SaveChangesAsync();
                _logger.LogInformation($"{addedDates} fechas faltantes agregadas a Dim_Date");
            }
            else
            {
                _logger.LogInformation($"Todas las fechas ya existen en Dim_Date");
            }
        }

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
    }
}