
using Core.Application.DTO.DimDto;

namespace Core.Application.Interfaces.ILoaders
{
    public interface ISurveyQuestionDimensionLoader
    {
        Task<int> LoadQuestionsAsync(IEnumerable<SurveyQuestionDto> questions);

        Task<int> GetOrCreateQuestionKeyAsync(string questionText);
    }
}
