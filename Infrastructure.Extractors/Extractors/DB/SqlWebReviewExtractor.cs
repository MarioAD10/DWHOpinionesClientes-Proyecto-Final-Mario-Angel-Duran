using Core.Application.DTO.DBDto;
using Core.Application.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Extractors.Extractors.DB
{
    public class SqlWebReviewExtractor : IExtractorGeneric<WebReviewDto>
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlWebReviewExtractor> _logger;

        public string SourceName => "DBWebReviews";

        public SqlWebReviewExtractor(string connectionString, ILogger<SqlWebReviewExtractor> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<IEnumerable<WebReviewDto>> ExtractAsync()
        {
            var allReviews = new List<WebReviewDto>();
            int batchSize = 1000;
            int offset = 0;
            bool hasMore = true;

            _logger.LogInformation("Iniciando extracción escalable desde DBWebReviews...");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                while (hasMore)
                {
                    var batch = await connection.QueryAsync<WebReviewDto>(
                        "sp_GetWebReviewsBatch",
                        new { BatchSize = batchSize, Offset = offset },
                        commandType: System.Data.CommandType.StoredProcedure);

                    var list = batch.ToList();
                    allReviews.AddRange(list);

                    _logger.LogInformation($"Lote {offset / batchSize + 1}: {list.Count} registros extraídos.");

                    if (list.Count < batchSize)
                        hasMore = false;
                    else
                        offset += batchSize;
                }

                _logger.LogInformation($"Extracción completada: {allReviews.Count} registros totales extraídos.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error durante la extracción desde DBWebReviews: {ex.Message}");
            }

            return allReviews;
        }
    }
}
