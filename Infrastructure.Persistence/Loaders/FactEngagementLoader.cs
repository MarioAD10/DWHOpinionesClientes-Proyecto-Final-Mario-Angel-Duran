using Core.Application.DTO.APIDto;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Infrastructure.Persistence.Entities.DWH.Fact;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la tabla de hechos Fact_Engagement.
    /// </summary>
    public class FactEngagementLoader : BaseDimensionLoader, IFactEngagementLoader
    {
        public FactEngagementLoader(
            DWOpinionesContext context,
            ILogger<FactEngagementLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga métricas de engagement desde la API de redes sociales.
        /// </summary>
        public async Task LoadEngagementAsync(IEnumerable<SocialCommentDto> apiRecords)
        {
            _logger.LogInformation("Iniciando carga de Fact_Engagement...");

            await ClearFactEngagementAsync();

            int totalInserted = 0;

            // ============================================
            // CARGAR DESDE API (Redes Sociales)
            // ============================================
            _logger.LogInformation($"Procesando {apiRecords.Count()} registros de la API...");
            totalInserted += await LoadFromApiAsync(apiRecords);

            _logger.LogInformation($"Fact_Engagement cargada: {totalInserted} registros insertados.");
        }

        private async Task ClearFactEngagementAsync()
        {
            _logger.LogInformation("Limpiando Fact_Engagement...");

            var existingRecords = await _context.FactEngagement.ToListAsync();

            if (existingRecords.Any())
            {
                _context.FactEngagement.RemoveRange(existingRecords);
                await SaveChangesAsync();
                _logger.LogInformation($"{existingRecords.Count} registros eliminados de Fact_Engagement");
            }
            else
            {
                _logger.LogInformation("Fact_Engagement ya está vacía");
            }
        }

        /// <summary>
        /// Carga engagement desde la API de redes sociales.
        /// </summary>
        private async Task<int> LoadFromApiAsync(IEnumerable<SocialCommentDto> apiRecords)
        {
            int insertedCount = 0;

            foreach (var record in apiRecords)
            {
                try
                {
                    // Obtener las Foreign Keys de las dimensiones
                    var customerKey = await GetCustomerKeyAsync(record.NombreCliente);
                    var productKey = await GetProductKeyAsync(record.NombreProducto, record.Marca, record.Categoria);
                    var dateKey = await GetDateKeyAsync(record.Fecha);
                    var sourceKey = await GetSourceKeyAsync("Redes Sociales API");
                    var channelKey = await GetChannelKeyAsync(record.Plataforma);

                    if (customerKey == -1 || productKey == -1 || dateKey == -1 ||
                        sourceKey == -1 || channelKey == -1)
                    {
                        _logger.LogWarning($"Registro {record.IdComentario} omitido por claves inválidas");
                        continue;
                    }

                    decimal engagementRate = record.Likes + record.Shares;
                    if (engagementRate > 0)
                    {
                        engagementRate = engagementRate / 10; 
                    }

                    // Crear el registro de Fact_Engagement
                    var factEngagement = new FactEngagementRecord
                    {
                        CustomerKey = customerKey,
                        ProductKey = productKey,
                        DateKey = dateKey,
                        SourceKey = sourceKey,
                        ChannelKey = channelKey,
                        ETLBatchKey = 1, 
                        LikesCount = record.Likes,
                        SharesCount = record.Shares,
                        CommentsCount = 1, 
                        ViewsCount = (record.Likes + record.Shares) * 10, 
                        RepliesCount = Math.Max(0, record.Likes / 5), 
                        EngagementRate = engagementRate,
                        IngestionTimestamp = DateTime.Now
                    };

                    await _context.FactEngagement.AddAsync(factEngagement);
                    insertedCount++;

                    // Guardar cada 50 registros
                    if (insertedCount % 50 == 0)
                    {
                        await SaveChangesAsync();
                        _logger.LogInformation($"{insertedCount} registros guardados de API...");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar registro API {record.IdComentario}: {ex.Message}");
                }
            }

            await SaveChangesAsync();
            return insertedCount;
        }

        // ============================================
        // MÉTODOS AUXILIARES PARA OBTENER FOREIGN KEYS
        // ============================================

        private async Task<int> GetCustomerKeyAsync(string customerName)
        {
            var customer = await _context.DimCustomer
                .FirstOrDefaultAsync(c => c.CustomerName == customerName);

            if (customer == null)
            {
                _logger.LogWarning($"Cliente no encontrado: {customerName}");
                return -1;
            }

            return customer.CustomerKey;
        }

        private async Task<int> GetProductKeyAsync(string productName, string brand, string category)
        {
            var product = await _context.DimProduct
                .FirstOrDefaultAsync(p =>
                    p.ProductName == productName &&
                    p.Brand == brand &&
                    p.Category == category);

            if (product == null)
            {
                _logger.LogWarning($"Producto no encontrado, creando: {productName} - {brand}");

                var newProduct = new DimProductRecord
                {
                    ProductName = productName,
                    Brand = brand ?? "Desconocido",
                    Category = category ?? "General",
                    Price = 0,
                    IsActive = true
                };

                await _context.DimProduct.AddAsync(newProduct);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Producto creado con Key: {newProduct.ProductKey}");
                return newProduct.ProductKey;
            }

            return product.ProductKey;
        }

        private async Task<int> GetDateKeyAsync(DateTime date)
        {
            var dateKey = int.Parse(date.ToString("yyyyMMdd"));

            var dateRecord = await _context.DimDate
                .FirstOrDefaultAsync(d => d.DateKey == dateKey);

            if (dateRecord == null)
            {
                _logger.LogWarning($"Fecha no encontrada: {date:yyyy-MM-dd}");
                return -1;
            }

            return dateKey;
        }

        private async Task<int> GetSourceKeyAsync(string sourceName)
        {
            var source = await _context.DimSource
                .FirstOrDefaultAsync(s => s.SourceName == sourceName);

            if (source == null)
            {
                _logger.LogWarning($"Fuente no encontrada: {sourceName}");
                return -1;
            }

            return source.SourceKey;
        }

        private async Task<int> GetChannelKeyAsync(string channelName)
        {
            var channel = await _context.DimChannel
                .FirstOrDefaultAsync(c => c.ChannelName == channelName);

            if (channel == null)
            {
                _logger.LogWarning($"Canal no encontrado: {channelName}");
                return -1;
            }

            return channel.ChannelKey;
        }
    }
}
