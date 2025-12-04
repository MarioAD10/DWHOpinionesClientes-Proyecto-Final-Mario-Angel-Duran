namespace Core.Application.Interfaces.ILoaders
{
    public interface IChannelDimensionLoader
    {
        Task<int> LoadChannelsAsync(IEnumerable<string> channelNames);
        Task<int> GetOrCreateChannelKeyAsync(string channelName);
    }
}
