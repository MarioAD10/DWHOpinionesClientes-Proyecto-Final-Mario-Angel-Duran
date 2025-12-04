using Core.Application.DTO.APIDto;
using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;
using Core.Application.Interfaces;
using Core.Application.Interfaces.ILoaders;

namespace DWHOpinionesClientes
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IExtractorGeneric<FactOpinionCsvDto> _csvExtractor;
        private readonly IExtractorGeneric<WebReviewDto> _sqlExtractor;
        private readonly IExtractorGeneric<SocialCommentDto> _apiExtractor;

        public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            IExtractorGeneric<FactOpinionCsvDto> csvExtractor,
            IExtractorGeneric<WebReviewDto> sqlExtractor,
            IExtractorGeneric<SocialCommentDto> apiExtractor)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _csvExtractor = csvExtractor;
            _sqlExtractor = sqlExtractor;
            _apiExtractor = apiExtractor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("------------------------------------------");
            _logger.LogInformation("INICIANDO PROCESO ETL - DWH OPINIONES DE CLIENTES");
            _logger.LogInformation("------------------------------------------");

            try
            {
                // ============================
                // FASE 1: EXTRACCIÓN (E)
                // ============================
                _logger.LogInformation("------------------------------------------");
                _logger.LogInformation("FASE 1: EXTRACCIÓN DE DATOS");
                _logger.LogInformation("------------------------------------------");

                var (csvRecords, dbRecords, apiRecords) = await ExtractDataAsync();

                // ============================
                // FASE 2: CARGA DE DIMENSIONES (L)
                // ============================
                _logger.LogInformation("------------------------------------------");
                _logger.LogInformation("FASE 2: CARGA DE DIMENSIONES");
                _logger.LogInformation("------------------------------------------");

                await LoadDimensionsAsync(csvRecords, dbRecords, apiRecords);

                // ============================
                // RESUMEN FINAL
                // ============================
                _logger.LogInformation("");
                _logger.LogInformation("------------------------------------------");
                _logger.LogInformation("PROCESO ETL COMPLETADO EXITOSAMENTE");
                _logger.LogInformation("------------------------------------------");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR EN EL PROCESO ETL: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// FASE 1: Extrae datos de las 3 fuentes
        /// </summary>
        private async Task<(IEnumerable<FactOpinionCsvDto>, IEnumerable<WebReviewDto>, IEnumerable<SocialCommentDto>)> ExtractDataAsync()
        {
            // Extracción desde CSV
            _logger.LogInformation("Extrayendo datos desde surveys_part1.csv...");
            var csvRecords = await _csvExtractor.ExtractAsync();
            _logger.LogInformation($"{csvRecords.Count()} registros extraídos del CSV");

            // Muestra de datos CSV ENRIQUECIDOS
            _logger.LogInformation("");
            _logger.LogInformation("Muestra de datos enriquecidos del CSV:");
            foreach (var item in csvRecords.Take(3))
            {
                _logger.LogInformation($"Cliente: {item.NombreCliente} ({item.Genero}, {item.RangoEdad}, {item.Pais})");
                _logger.LogInformation($"    Producto: {item.NombreProducto} - {item.Marca} ({item.Categoria}) - ${item.Precio}");
            }

            // Extracción desde Base de Datos
            _logger.LogInformation("");
            _logger.LogInformation("Extrayendo datos desde DBWebReview...");
            var dbRecords = await _sqlExtractor.ExtractAsync();
            _logger.LogInformation($"{dbRecords.Count()} registros extraídos de la base de datos");

            _logger.LogInformation("");
            _logger.LogInformation("Muestra de datos enriquecidos de DBWebReview:");
            foreach (var item in dbRecords.Take(3))
            {
                _logger.LogInformation($"Cliente: {item.NombreCliente} ({item.Genero}, {item.RangoEdad}, {item.Pais})");
                _logger.LogInformation($"    Producto: {item.NombreProducto} - {item.Marca} ({item.Categoria}) - ${item.Precio}");
            }

            // Extracción desde API REST
            _logger.LogInformation("");
            _logger.LogInformation("Extrayendo datos desde API REST (Social Comments)...");
            var apiRecords = await _apiExtractor.ExtractAsync();
            _logger.LogInformation($"{apiRecords.Count()} registros extraídos de la API");

            _logger.LogInformation("");
            _logger.LogInformation("Muestra de datos enriquecidos del API:");
            foreach (var item in apiRecords.Take(3))
            {
                _logger.LogInformation($"Cliente: {item.NombreCliente} ({item.Genero}, {item.RangoEdad}, {item.Pais})");
                _logger.LogInformation($"Producto: {item.NombreProducto} - {item.Marca} ({item.Categoria}) - ${item.Precio}");
                _logger.LogInformation($"Plataforma: {item.Plataforma}, Likes: {item.Likes}");
            }

            return (csvRecords, dbRecords, apiRecords);
        }

        /// <summary>
        /// FASE 2: Carga todas las dimensiones del DWH
        /// </summary>
        private async Task LoadDimensionsAsync(
            IEnumerable<FactOpinionCsvDto> csvRecords,
            IEnumerable<WebReviewDto> dbRecords,
            IEnumerable<SocialCommentDto> apiRecords)
        {
            using var scope = _serviceProvider.CreateScope();

            var sentimentLoader = scope.ServiceProvider.GetRequiredService<ISentimentDimensionLoader>();
            var sourceLoader = scope.ServiceProvider.GetRequiredService<ISourceDimensionLoader>();
            var channelLoader = scope.ServiceProvider.GetRequiredService<IChannelDimensionLoader>();
            var dateLoader = scope.ServiceProvider.GetRequiredService<IDateDimensionLoader>();
            var productLoader = scope.ServiceProvider.GetRequiredService<IProductDimensionLoader>();
            var customerLoader = scope.ServiceProvider.GetRequiredService<ICustomerDimensionLoader>();

            // ------------------------------------------
            //  CARGAR CATÁLOGO DE SENTIMIENTOS
            // ------------------------------------------
            _logger.LogInformation("");
            _logger.LogInformation("Cargando catálogo de Sentimientos...");
            await sentimentLoader.InitializeSentimentCatalogAsync();

            // ------------------------------------------
            //  CARGAR FUENTES DE DATOS
            // ------------------------------------------
            _logger.LogInformation("");
            _logger.LogInformation("Cargando Fuentes de datos...");
            var sources = new[] { "Encuestas CSV", "Reseñas Web", "Redes Sociales API" };
            await sourceLoader.LoadSourcesAsync(sources);

            // ------------------------------------------
            //  CARGAR CANALES
            // ------------------------------------------
            _logger.LogInformation("");
            _logger.LogInformation("Cargando Canales...");
            var channels = new[] { "Web", "Móvil", "Facebook", "Twitter", "Instagram", "Encuesta Online" };
            await channelLoader.LoadChannelsAsync(channels);

            // ------------------------------------------
            //  CARGAR DIMENSIÓN DE FECHAS
            // ------------------------------------------
            _logger.LogInformation("");
            _logger.LogInformation("Cargando dimensión de Fechas...");


            var allDates = new List<DateTime>();
            allDates.AddRange(csvRecords.Select(r => r.Fecha));
            allDates.AddRange(dbRecords.Select(r => r.Fecha));
            allDates.AddRange(apiRecords.Select(r => r.Fecha));

            if (allDates.Any())
            {
                var minDate = allDates.Min().Date;
                var maxDate = allDates.Max().Date;

                _logger.LogInformation($"Rango de fechas detectado: {minDate:yyyy-MM-dd} a {maxDate:yyyy-MM-dd}");
                await dateLoader.LoadDateRangeAsync(minDate, maxDate);
            }
            else
            {
                _logger.LogWarning("No se encontraron fechas en los datos extraídos");
            }

            // ------------------------------------------
            // CARGAR PRODUCTOS (CON DATOS ENRIQUECIDOS)
            // ------------------------------------------
            _logger.LogInformation("");
            _logger.LogInformation("Cargando Productos con datos enriquecidos...");

            var products = new List<Core.Application.DTO.DimDto.ProductSourceDto>();

            var uniqueProducts = csvRecords
                .GroupBy(r => new { r.IdProducto, r.NombreProducto, r.Marca, r.Categoria })
                .Select(g => g.First())
                .ToList();

            foreach (var record in uniqueProducts)
            {
                products.Add(new Core.Application.DTO.DimDto.ProductSourceDto
                {
                    ProductName = record.NombreProducto,      
                    Brand = record.Marca,                    
                    Category = record.Categoria,             
                    Price = record.Precio,                  
                    IsActive = true
                });
            }

            // Agregar productos de DB 
            var dbProductIds = dbRecords
                .Select(r => r.IdProducto)
                .Distinct()
                .Except(csvRecords.Select(r => r.IdProducto))
                .ToList();

            foreach (var productId in dbProductIds)
            {
                products.Add(new Core.Application.DTO.DimDto.ProductSourceDto
                {
                    ProductName = $"Producto {productId}",
                    Brand = "Marca Genérica",
                    Category = "Categoría General",
                    Price = 0,
                    IsActive = true
                });
            }

            // Agregar productos de API 
            var apiProductIds = apiRecords
                .Select(r => r.IdProducto)
                .Distinct()
                .Except(csvRecords.Select(r => r.IdProducto))
                .Except(dbRecords.Select(r => r.IdProducto))
                .ToList();

            foreach (var productId in apiProductIds)
            {
                products.Add(new Core.Application.DTO.DimDto.ProductSourceDto
                {
                    ProductName = $"Producto {productId}",
                    Brand = "Marca Genérica",
                    Category = "Categoría General",
                    Price = 0,
                    IsActive = true
                });
            }

            _logger.LogInformation($"{products.Count} productos únicos detectados ({uniqueProducts.Count} enriquecidos, {products.Count - uniqueProducts.Count} genéricos)");
            await productLoader.LoadProductsAsync(products);

            // ------------------------------------------
            // CARGAR CLIENTES (CON DATOS ENRIQUECIDOS)
            // ------------------------------------------
            _logger.LogInformation("");
            _logger.LogInformation("Cargando Clientes con datos enriquecidos...");

            var customers = new List<Core.Application.DTO.DimDto.CustomerSourceDto>();

            // CAMBIO CRÍTICO: Usar datos enriquecidos del CSV
            var uniqueCustomers = csvRecords
                .GroupBy(r => new { r.IdCliente, r.NombreCliente })
                .Select(g => g.First())
                .ToList();

            foreach (var record in uniqueCustomers)
            {
                customers.Add(new Core.Application.DTO.DimDto.CustomerSourceDto
                {
                    CustomerName = record.NombreCliente,      
                    Gender = record.Genero,                   
                    AgeRange = record.RangoEdad,             
                    Country = record.Pais                     
                });
            }

            // Agregar clientes de DB
            var dbCustomerIds = dbRecords
                .Select(r => r.IdCliente)
                .Distinct()
                .Except(csvRecords.Select(r => r.IdCliente))
                .ToList();

            foreach (var customerId in dbCustomerIds)
            {
                customers.Add(new Core.Application.DTO.DimDto.CustomerSourceDto
                {
                    CustomerName = $"Cliente {customerId}",
                    Gender = "Desconocido",
                    AgeRange = "Desconocido",
                    Country = "Desconocido"
                });
            }

            // Agregar clientes de API 
            var apiCustomerIds = apiRecords
                .Select(r => r.IdCliente)
                .Distinct()
                .Except(csvRecords.Select(r => r.IdCliente))
                .Except(dbRecords.Select(r => r.IdCliente))
                .ToList();

            foreach (var customerId in apiCustomerIds)
            {
                customers.Add(new Core.Application.DTO.DimDto.CustomerSourceDto
                {
                    CustomerName = $"Cliente {customerId}",
                    Gender = "Desconocido",
                    AgeRange = "Desconocido",
                    Country = "Desconocido"
                });
            }

            _logger.LogInformation($"{customers.Count} clientes únicos detectados ({uniqueCustomers.Count} enriquecidos, {customers.Count - uniqueCustomers.Count} genéricos)");
            await customerLoader.LoadCustomersAsync(customers);

            _logger.LogInformation("");
            _logger.LogInformation("Todas las dimensiones han sido cargadas exitosamente");
        }
    }
}
