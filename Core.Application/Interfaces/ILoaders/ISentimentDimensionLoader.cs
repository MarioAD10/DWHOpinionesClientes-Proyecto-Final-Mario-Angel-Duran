namespace Core.Application.Interfaces.ILoaders
{
    public interface ISentimentDimensionLoader
    {
        Task InitializeSentimentCatalogAsync();
        Task<int> GetSentimentKeyAsync(string sentimentName);
    }
}
