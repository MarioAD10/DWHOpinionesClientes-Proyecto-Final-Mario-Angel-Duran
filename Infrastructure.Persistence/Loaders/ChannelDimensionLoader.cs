using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    // <summary>
    /// Loader para la dimensión de canales (Web, Móvil, Redes Sociales, etc.).
    /// </summary>
    public class ChannelDimensionLoader : BaseDimensionLoader, IChannelDimensionLoader
    {
        public ChannelDimensionLoader(
            DWOpinionesContext context,
            ILogger<ChannelDimensionLoader> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Carga múltiples canales en la dimensión.
        /// </summary>
        public async Task<int> LoadChannelsAsync(IEnumerable<string> channelNames)
        {
            if (channelNames == null || !channelNames.Any())
            {
                _logger.LogWarning("No hay canales para cargar.");
                return 0;
            }

            _logger.LogInformation($"Cargando {channelNames.Count()} canales...");

            int insertedCount = 0;

            foreach (var channelName in channelNames.Distinct())
            {
                var exists = await _context.DimChannel
                    .AnyAsync(c => c.ChannelName == channelName);

                if (!exists)
                {
                    var newChannel = new DimChannelRecord
                    {
                        ChannelName = channelName,
                        ChannelType = DetermineChannelType(channelName)
                    };

                    await _context.DimChannel.AddAsync(newChannel);
                    insertedCount++;
                }
            }

            if (insertedCount > 0)
            {
                await SaveChangesAsync();
                _logger.LogInformation($"{insertedCount} canales cargados.");
            }
            else
            {
                _logger.LogInformation("Todos los canales ya existen.");
            }

            return insertedCount;
        }

        /// <summary>
        /// Obtiene o crea la clave de un canal.
        /// </summary>
        public async Task<int> GetOrCreateChannelKeyAsync(string channelName)
        {
            var channel = await _context.DimChannel
                .FirstOrDefaultAsync(c => c.ChannelName == channelName);

            if (channel != null)
            {
                return channel.ChannelKey;
            }

            // Crear nuevo canal
            var newChannel = new DimChannelRecord
            {
                ChannelName = channelName,
                ChannelType = DetermineChannelType(channelName)
            };

            await _context.DimChannel.AddAsync(newChannel);
            await SaveChangesAsync();

            _logger.LogInformation($"Nuevo canal creado: {channelName} - Key: {newChannel.ChannelKey}");
            return newChannel.ChannelKey;
        }

        /// <summary>
        /// Determina el tipo de canal basado en su nombre.
        /// </summary>
        private string DetermineChannelType(string channelName)
        {
            var lower = channelName.ToLower();

            if (lower.Contains("facebook") || lower.Contains("twitter") ||
                lower.Contains("instagram") || lower.Contains("social"))
                return "Red Social";

            if (lower.Contains("web") || lower.Contains("sitio"))
                return "Sitio Web";

            if (lower.Contains("encuesta") || lower.Contains("survey"))
                return "Encuesta";

            if (lower.Contains("email") || lower.Contains("correo"))
                return "Email";

            if (lower.Contains("móvil") || lower.Contains("movil") || lower.Contains("app"))
                return "Aplicación Móvil";

            return "Otro";
        }
    }
}
