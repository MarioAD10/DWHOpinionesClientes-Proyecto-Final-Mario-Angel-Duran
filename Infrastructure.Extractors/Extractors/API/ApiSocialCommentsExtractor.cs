using Core.Application.DTO.APIDto;
using Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.Extractors.Extractors.API
{
    public class ApiSocialCommentsExtractor : IExtractorGeneric<SocialCommentDto>
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiSocialCommentsExtractor> _logger;
        private readonly string _apiUrl;

        public ApiSocialCommentsExtractor(string apiUrl, ILogger<ApiSocialCommentsExtractor> logger)
        {
            _httpClient = new HttpClient();
            _apiUrl = apiUrl;
            _logger = logger;
        }

        public string SourceName => "API SocialComments";

        public async Task<IEnumerable<SocialCommentDto>> ExtractAsync()
        {
            var results = new List<SocialCommentDto>();

            try
            {
                _logger.LogInformation($"Solicitando datos desde API: {_apiUrl}");

                var response = await _httpClient.GetAsync(_apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var comments = await response.Content.ReadFromJsonAsync<List<SocialCommentDto>>();
                    if (comments != null)
                    {
                        results = comments;
                        _logger.LogInformation($"Se extrajeron {results.Count} registros desde el API REST.");
                    }
                }
                else
                {
                    _logger.LogWarning($"Error en la solicitud: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extrayendo datos del API: {ex.Message}");
            }

            return results;
        }
    }
}
