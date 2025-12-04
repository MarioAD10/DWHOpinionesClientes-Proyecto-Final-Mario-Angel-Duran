using Core.Application.DTO;

namespace Core.Application.Interfaces
{
    public interface IStagingRepository
    {
        Task<int> SaveFactOpinionAsync(IEnumerable<FactOpinionDto> records);
        Task<int> SaveFactSurveyResponseAsync(IEnumerable<FactSurveyResponseDto> records);
        Task<int> SaveFactEngagementAsync(IEnumerable<FactEngagementDto> records);
        Task<int> SaveFactProductSummaryAsync(IEnumerable<FactProductSummaryDto> records);
        Task ClearTableAsync(string tableName);


    }
}
