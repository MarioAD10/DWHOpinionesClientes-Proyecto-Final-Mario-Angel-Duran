using Core.Application.DTO.APIDto;
using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;

namespace Core.Application.Interfaces.ILoaders
{
    public interface IFactOpinionLoader
    {
        Task LoadOpinionsAsync(
            IEnumerable<FactOpinionCsvDto> csvRecords,
            IEnumerable<WebReviewDto> dbRecords,
            IEnumerable<SocialCommentDto> apiRecords);
    }
}
