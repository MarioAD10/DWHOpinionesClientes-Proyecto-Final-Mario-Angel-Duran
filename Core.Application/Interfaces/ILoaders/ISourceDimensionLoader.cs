namespace Core.Application.Interfaces.ILoaders
{
    public interface ISourceDimensionLoader
    {
        Task<int> LoadSourcesAsync(IEnumerable<string> sourceNames);
        Task<int> GetOrCreateSourceKeyAsync(string sourceName);
    }
}
