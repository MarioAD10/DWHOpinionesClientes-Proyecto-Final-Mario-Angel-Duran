using Core.Application.DTO.DimDto;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la dimensión de productos.
    /// </summary>
    public class ProductDimensionLoader : BaseDimensionLoader, IProductDimensionLoader
    {
        public ProductDimensionLoader(
            DWOpinionesContext context,
            ILogger<ProductDimensionLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga múltiples productos en la dimensión.
        /// </summary>
        public async Task<int> LoadProductsAsync(IEnumerable<ProductSourceDto> products)
        {
            if (products == null || !products.Any())
            {
                _logger.LogWarning("No hay productos para cargar.");
                return 0;
            }

            _logger.LogInformation($"Iniciando carga de {products.Count()} productos...");

            int insertedCount = 0;
            int updatedCount = 0;

            foreach (var product in products)
            {
                // Validar datos básicos
                if (string.IsNullOrWhiteSpace(product.ProductName))
                {
                    _logger.LogWarning("Producto sin nombre, omitiendo...");
                    continue;
                }

                // Buscar producto existente por nombre, marca y categoría
                // (la combinación de estos 3 campos identifica únicamente un producto)
                var existingProduct = await _context.DimProduct
                    .FirstOrDefaultAsync(p =>
                        p.ProductName == product.ProductName &&
                        p.Brand == product.Brand &&
                        p.Category == product.Category);

                if (existingProduct == null)
                {
                    // Insertar nuevo producto
                    var newProduct = new DimProductRecord
                    {
                        ProductName = product.ProductName,
                        Brand = product.Brand ?? "Genérico",
                        Category = product.Category ?? "Sin categoría",
                        Price = product.Price,
                        IsActive = product.IsActive
                    };

                    await _context.DimProduct.AddAsync(newProduct);
                    insertedCount++;
                }
                else
                {
                    // Actualizar producto existente 
                    existingProduct.Price = product.Price;
                    existingProduct.IsActive = product.IsActive;
                    updatedCount++;
                }

                // Guardar cada 50 registros
                if ((insertedCount + updatedCount) % 50 == 0)
                {
                    await SaveChangesAsync();
                }
            }

            // Guardar cambios restantes
            await SaveChangesAsync();

            _logger.LogInformation($"Productos cargados: {insertedCount} insertados, {updatedCount} actualizados.");
            return insertedCount + updatedCount;
        }

        /// <summary>
        /// Obtiene o crea la clave de un producto específico.
        /// </summary>
        public async Task<int> GetOrCreateProductKeyAsync(string productName, string brand, string category)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                _logger.LogWarning("Intento de buscar producto sin nombre.");
                return -1; // O lanzar excepción según tu lógica
            }

            var product = await _context.DimProduct
                .FirstOrDefaultAsync(p =>
                    p.ProductName == productName &&
                    p.Brand == brand &&
                    p.Category == category);

            if (product != null)
            {
                return product.ProductKey;
            }

            // Crear nuevo producto
            var newProduct = new DimProductRecord
            {
                ProductName = productName,
                Brand = brand ?? "Genérico",
                Category = category ?? "Sin categoría",
                Price = 0,
                IsActive = true
            };

            await _context.DimProduct.AddAsync(newProduct);
            await SaveChangesAsync();

            _logger.LogInformation($"🆕 Nuevo producto creado: {productName} - Key: {newProduct.ProductKey}");
            return newProduct.ProductKey;
        }
    }
}
