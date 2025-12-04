using Core.Application.DTO.DimDto;

namespace Core.Application.Interfaces.ILoaders
{
    public interface ICustomerDimensionLoader
    {
        Task<int> LoadCustomersAsync(IEnumerable<CustomerSourceDto> customers);
        Task<int> GetOrCreateCustomerKeyAsync(string customerName, string gender, string ageRange, string country);
    }
}

