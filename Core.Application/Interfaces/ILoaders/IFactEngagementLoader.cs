using Core.Application.DTO.APIDto;

namespace Core.Application.Interfaces.ILoaders
{
    public interface IFactEngagementLoader
    {
        Task LoadEngagementAsync(IEnumerable<SocialCommentDto> apiRecords);
    }
}
