using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la dimensión de sentimientos (Positivo, Negativo, Neutro).
    /// </summary>
    public class SentimentDimensionLoader : BaseDimensionLoader, ISentimentDimensionLoader
    {
        public SentimentDimensionLoader(
            DWOpinionesContext context,
            ILogger<SentimentDimensionLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Inicializa el catálogo de sentimientos (se ejecuta una vez al inicio).
        /// </summary>
        public async Task InitializeSentimentCatalogAsync()
        {
            _logger.LogInformation("Inicializando catálogo de sentimientos...");

            var sentiments = new[]
            {
                new { Name = "Positivo", Polarity = 1 },
                new { Name = "Negativo", Polarity = -1 },
                new { Name = "Neutro", Polarity = 0 }
            };

            int insertedCount = 0;

            foreach (var sentiment in sentiments)
            {
                var exists = await _context.DimSentiment
                    .AnyAsync(s => s.SentimentName == sentiment.Name);

                if (!exists)
                {
                    var newSentiment = new DimSentimentRecord
                    {
                        SentimentName = sentiment.Name,
                        Polarity = sentiment.Polarity
                    };

                    await _context.DimSentiment.AddAsync(newSentiment);
                    insertedCount++;
                }
            }

            if (insertedCount > 0)
            {
                await SaveChangesAsync();
                _logger.LogInformation($"{insertedCount} sentimientos inicializados.");
            }
            else
            {
                _logger.LogInformation("Catálogo de sentimientos ya existe.");
            }
        }

        /// <summary>
        /// Obtiene la clave de un sentimiento por su nombre.
        /// </summary>
        public async Task<int> GetSentimentKeyAsync(string sentimentName)
        {
            var sentiment = await _context.DimSentiment
                .FirstOrDefaultAsync(s => s.SentimentName == sentimentName);

            if (sentiment == null)
            {
                _logger.LogWarning($"Sentimiento '{sentimentName}' no encontrado. Usando 'Neutro' por defecto.");
                sentiment = await _context.DimSentiment
                    .FirstOrDefaultAsync(s => s.SentimentName == "Neutro");
            }

            return sentiment?.SentimentKey ?? 3; // Fallback
        }
    }
}
