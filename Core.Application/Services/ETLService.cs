using Core.Application.Interfaces;
using Core.Application.Models;

namespace Core.Application.Services
{
    public class ETLService
    {
        private readonly IEnumerable<IExtractor> _extractors;
        private readonly IETLLogger _logger;

        public ETLService(IEnumerable<IExtractor> extractors, IETLLogger logger)
        {
            _extractors = extractors;
            _logger = logger;
        }

        /// <summary>
        /// Ejecuta todos los extractores registrados en el sistema.
        /// </summary>
        public async Task RunExtractionAsync()
        {
            foreach (var extractor in _extractors)
            {
                var parameters = new ExtractionParams
                {
                    SourceType = extractor.SourceName
                };

                _logger.Info($"Iniciando extracción para {extractor.SourceName}...");

                var result = await extractor.ExtractAsync(parameters);

                if (result.Success)
                    _logger.Info($"✅ {extractor.SourceName}: {result.RecordsInserted} registros insertados en staging.");
                else
                    _logger.Error($"❌ Error en {extractor.SourceName}: {result.Message}");
            }
        }
    }
}
