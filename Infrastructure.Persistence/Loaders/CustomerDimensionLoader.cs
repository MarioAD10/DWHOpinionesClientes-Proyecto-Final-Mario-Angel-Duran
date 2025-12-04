using Core.Application.DTO.DimDto;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la dimensión de clientes.
    /// </summary>
    public class CustomerDimensionLoader : BaseDimensionLoader, ICustomerDimensionLoader
    {
        public CustomerDimensionLoader(
            DWOpinionesContext context,
            ILogger<CustomerDimensionLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga múltiples clientes en la dimensión.
        /// </summary>
        public async Task<int> LoadCustomersAsync(IEnumerable<CustomerSourceDto> customers)
        {
            if (customers == null || !customers.Any())
            {
                _logger.LogWarning("No hay clientes para cargar.");
                return 0;
            }

            _logger.LogInformation($"Iniciando carga de {customers.Count()} clientes...");

            int insertedCount = 0;
            int updatedCount = 0;

            foreach (var customer in customers)
            {
                // Validar datos básicos
                if (string.IsNullOrWhiteSpace(customer.CustomerName))
                {
                    _logger.LogWarning("Cliente sin nombre, omitiendo...");
                    continue;
                }

                // Buscar cliente existente por nombre
                var existingCustomer = await _context.DimCustomer
                    .FirstOrDefaultAsync(c => c.CustomerName == customer.CustomerName);

                if (existingCustomer == null)
                {
                    // Insertar nuevo cliente
                    var newCustomer = new DimCustomerRecord
                    {
                        CustomerName = customer.CustomerName,
                        Gender = customer.Gender ?? "Desconocido",
                        AgeRange = customer.AgeRange ?? "Desconocido",
                        Country = customer.Country ?? "Desconocido"
                    };

                    await _context.DimCustomer.AddAsync(newCustomer);
                    insertedCount++;
                }
                else
                {
                    // Actualizar cliente existente (SCD Type 1)
                    existingCustomer.Gender = customer.Gender ?? existingCustomer.Gender;
                    existingCustomer.AgeRange = customer.AgeRange ?? existingCustomer.AgeRange;
                    existingCustomer.Country = customer.Country ?? existingCustomer.Country;
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

            _logger.LogInformation($"Clientes cargados: {insertedCount} insertados, {updatedCount} actualizados.");
            return insertedCount + updatedCount;
        }

        /// <summary>
        /// Obtiene o crea la clave de un cliente específico.
        /// </summary>
        public async Task<int> GetOrCreateCustomerKeyAsync(string customerName, string gender, string ageRange, string country)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                _logger.LogWarning("Intento de buscar cliente sin nombre.");
                return -1; // O lanzar excepción según tu lógica
            }

            var customer = await _context.DimCustomer
                .FirstOrDefaultAsync(c => c.CustomerName == customerName);

            if (customer != null)
            {
                return customer.CustomerKey;
            }

            // Crear nuevo cliente
            var newCustomer = new DimCustomerRecord
            {
                CustomerName = customerName,
                Gender = gender ?? "Desconocido",
                AgeRange = ageRange ?? "Desconocido",
                Country = country ?? "Desconocido"
            };

            await _context.DimCustomer.AddAsync(newCustomer);
            await SaveChangesAsync();

            _logger.LogInformation($"Nuevo cliente creado: {customerName} - Key: {newCustomer.CustomerKey}");
            return newCustomer.CustomerKey;
        }
    }
}
