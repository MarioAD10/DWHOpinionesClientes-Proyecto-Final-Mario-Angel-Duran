using Core.Application.DTO.APIDto;
using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;
using Core.Application.Interfaces;

namespace DWHOpinionesClientes
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IExtractorGeneric<FactOpinionCsvDto> _csvExtractor;
        private readonly IExtractorGeneric<WebReviewDto> _sqlExtractor;
        private readonly IExtractorGeneric<SocialCommentDto> _apiExtractor;

        public Worker(
            ILogger<Worker> logger,
            IExtractorGeneric<FactOpinionCsvDto> csvExtractor,
            IExtractorGeneric<WebReviewDto> sqlExtractor,
            IExtractorGeneric<SocialCommentDto> apiExtractor)
        {
            _logger = logger;
            _csvExtractor = csvExtractor;
            _sqlExtractor = sqlExtractor;
            _apiExtractor = apiExtractor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando proceso de extracción...");

            // ============================
            // EXTRACCIÓN DESDE CSV
            // ============================
            _logger.LogInformation("Iniciando extracción desde surveys_part1.csv...");
            var registrosCsv = await _csvExtractor.ExtractAsync();
            _logger.LogInformation($"Se extrajeron {registrosCsv.Count()} registros válidos del CSV.");

            foreach (var item in registrosCsv.Take(3))
            {
                _logger.LogInformation($"[CSV] Opinión {item.IdOpinion}: Cliente {item.IdCliente}, Producto {item.IdProducto}, Satisfacción {item.PuntajeSatisfacción}");
            }

            // ============================
            // EXTRACCIÓN DESDE BASE DE DATOS RELACIONAL
            // ============================
            _logger.LogInformation("Iniciando extracción desde DBWebReview...");
            var registrosDb = await _sqlExtractor.ExtractAsync();
            _logger.LogInformation($"Se extrajeron {registrosDb.Count()} registros válidos desde la base de datos.");

            foreach (var item in registrosDb.Take(3))
            {
                _logger.LogInformation($"[SQL] Reseña {item.IdReseña}: Cliente {item.IdCliente}, Producto {item.IdProducto}, Puntaje {item.Puntaje}");
            }

            // ============================
            // EXTRACCIÓN API REST
            // ============================

            _logger.LogInformation("Iniciando extracción desde API REST (Social Comments)...");
            var registrosApi = await _apiExtractor.ExtractAsync();
            _logger.LogInformation($"API REST: {registrosApi.Count()} registros extraídos.");

            foreach (var item in registrosApi.Take(3))
            {
                _logger.LogInformation($"Comentario {item.IdComentario}: Cliente {item.IdCliente}, Producto {item.IdProducto}, Likes {item.Likes}, Plataforma {item.Plataforma}");
            }

            _logger.LogInformation("Proceso de extracción completado exitosamente (CSV + SQL + API).");
        }
    }
}
