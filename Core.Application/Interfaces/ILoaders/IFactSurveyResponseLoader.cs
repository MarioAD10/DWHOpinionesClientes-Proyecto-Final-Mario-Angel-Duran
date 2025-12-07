using Core.Application.DTO.CSVDto;

namespace Core.Application.Interfaces.ILoaders
{
    public interface IFactSurveyResponseLoader
    {
        Task LoadSurveyResponsesAsync(IEnumerable<FactOpinionCsvDto> csvRecords);
    }
}
