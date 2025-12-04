using Core.Application.DTO.DimDto;

namespace Core.Application.Interfaces.ILoaders
{
    public interface IProductDimensionLoader
    {
        Task<int> LoadProductsAsync(IEnumerable<ProductSourceDto> products);
        Task<int> GetOrCreateProductKeyAsync(string productName, string brand, string category);
    }
}
