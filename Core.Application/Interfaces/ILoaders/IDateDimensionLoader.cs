namespace Core.Application.Interfaces.ILoaders
{
    public interface IDateDimensionLoader
    {
        Task<int> LoadDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrCreateDateKeyAsync(DateTime date);
    }
}
