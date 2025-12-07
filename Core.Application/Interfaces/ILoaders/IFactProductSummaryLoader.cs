using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;
using Core.Application.DTO.APIDto;

namespace Core.Application.Interfaces.ILoaders
{
    public interface IFactProductSummaryLoader
    {
        Task LoadProductSummaryAsync(
            IEnumerable<FactOpinionCsvDto> csvRecords,
            IEnumerable<WebReviewDto> dbRecords,
            IEnumerable<SocialCommentDto> apiRecords);
    }
}
