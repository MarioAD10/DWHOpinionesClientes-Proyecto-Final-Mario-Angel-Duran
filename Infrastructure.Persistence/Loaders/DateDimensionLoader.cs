using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la dimensión de fechas.
    /// Genera automáticamente un rango completo de fechas con sus atributos.
    /// </summary>
    public class DateDimensionLoader : BaseDimensionLoader, IDateDimensionLoader
    {
        public DateDimensionLoader(
            DWOpinionesContext context,
            ILogger<DateDimensionLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga un rango de fechas en la dimensión (ej: todo el año 2024).
        /// </summary>
        public async Task<int> LoadDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation($"Cargando dimensión de fechas desde {startDate:yyyy-MM-dd} hasta {endDate:yyyy-MM-dd}...");

            int insertedCount = 0;
            var cultureDR = new CultureInfo("es-DO"); // Cultura dominicana

            // Iterar día por día
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Verificar si la fecha ya existe
                var exists = await _context.DimDate
                    .AnyAsync(d => d.FullDate == date);

                if (!exists)
                {
                    var dimDate = new DimDateRecord
                    {
                        FullDate = date,
                        Year = date.Year,
                        Quarter = (byte)((date.Month - 1) / 3 + 1),
                        Month = (byte)date.Month,
                        MonthName = cultureDR.DateTimeFormat.GetMonthName(date.Month),
                        Day = (byte)date.Day
                    };

                    await _context.DimDate.AddAsync(dimDate);
                    insertedCount++;

                    // Guardar cada 100 registros para optimizar memoria
                    if (insertedCount % 100 == 0)
                    {
                        await SaveChangesAsync();
                        _logger.LogInformation($"   💾 {insertedCount} fechas guardadas...");
                    }
                }
            }

            // Guardar registros restantes
            if (insertedCount % 100 != 0)
            {
                await SaveChangesAsync();
            }

            _logger.LogInformation($"✅ Dimensión de fechas cargada: {insertedCount} fechas nuevas.");
            return insertedCount;
        }

        /// <summary>
        /// Obtiene o crea la clave de una fecha específica.
        /// </summary>
        public async Task<int> GetOrCreateDateKeyAsync(DateTime date)
        {
            date = date.Date; // Normalizar a medianoche (ignorar hora)

            var dimDate = await _context.DimDate
                .FirstOrDefaultAsync(d => d.FullDate == date);

            if (dimDate != null)
            {
                return dimDate.DateKey;
            }

            // Si no existe, crear la fecha
            var cultureDR = new CultureInfo("es-DO");

            var newDate = new DimDateRecord
            {
                FullDate = date,
                Year = date.Year,
                Quarter = (byte)((date.Month - 1) / 3 + 1),
                Month = (byte)date.Month,
                MonthName = cultureDR.DateTimeFormat.GetMonthName(date.Month),
                Day = (byte)date.Day
            };

            await _context.DimDate.AddAsync(newDate);
            await SaveChangesAsync();

            _logger.LogInformation($"🆕 Nueva fecha creada: {date:yyyy-MM-dd} - Key: {newDate.DateKey}");
            return newDate.DateKey;
        }
    }
}
