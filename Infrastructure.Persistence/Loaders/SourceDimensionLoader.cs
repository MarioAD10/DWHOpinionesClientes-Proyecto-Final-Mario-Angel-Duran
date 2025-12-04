using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la dimensión de fuentes de datos (CSV, API, Web).
    /// </summary>
    public class SourceDimensionLoader : BaseDimensionLoader, ISourceDimensionLoader
    {
        public SourceDimensionLoader(
            DWOpinionesContext context,
            ILogger<SourceDimensionLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga múltiples fuentes en la dimensión.
        /// </summary>
        public async Task<int> LoadSourcesAsync(IEnumerable<string> sourceNames)
        {
            if (sourceNames == null || !sourceNames.Any())
            {
                _logger.LogWarning("No hay fuentes para cargar.");
                return 0;
            }

            _logger.LogInformation($"Cargando {sourceNames.Count()} fuentes...");

            int insertedCount = 0;

            foreach (var sourceName in sourceNames.Distinct())
            {
                var exists = await _context.DimSource
                    .AnyAsync(s => s.SourceName == sourceName);

                if (!exists)
                {
                    var newSource = new DimSourceRecord
                    {
                        SourceName = sourceName,
                        SourceType = DetermineSourceType(sourceName)
                    };

                    await _context.DimSource.AddAsync(newSource);
                    insertedCount++;
                }
            }

            if (insertedCount > 0)
            {
                await SaveChangesAsync();
                _logger.LogInformation($"{insertedCount} fuentes cargadas.");
            }
            else
            {
                _logger.LogInformation("Todas las fuentes ya existen.");
            }

            return insertedCount;
        }

        /// <summary>
        /// Obtiene o crea la clave de una fuente.
        /// </summary>
        public async Task<int> GetOrCreateSourceKeyAsync(string sourceName)
        {
            var source = await _context.DimSource
                .FirstOrDefaultAsync(s => s.SourceName == sourceName);

            if (source != null)
            {
                return source.SourceKey;
            }

            // Crear nueva fuente
            var newSource = new DimSourceRecord
            {
                SourceName = sourceName,
                SourceType = DetermineSourceType(sourceName)
            };

            await _context.DimSource.AddAsync(newSource);
            await SaveChangesAsync();

            _logger.LogInformation($"Nueva fuente creada: {sourceName} - Key: {newSource.SourceKey}");
            return newSource.SourceKey;
        }

        /// <summary>
        /// Determina el tipo de fuente basado en su nombre.
        /// </summary>
        private string DetermineSourceType(string sourceName)
        {
            var lower = sourceName.ToLower();

            if (lower.Contains("csv") || lower.Contains("encuesta"))
                return "Archivo CSV";
            if (lower.Contains("web") || lower.Contains("review"))
                return "Base de Datos";
            if (lower.Contains("api") || lower.Contains("social"))
                return "API REST";

            return "Desconocido";
        }
    }
}
