using Infrastructure.Persistence.Context;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Clase base abstracta para todos los loaders de dimensiones.
    /// Contiene lógica común como manejo del contexto y logging.
    /// </summary>
    public abstract class BaseDimensionLoader
    {
        protected readonly DWOpinionesContext _context;
        protected readonly ILogger _logger;

        protected BaseDimensionLoader(DWOpinionesContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Guarda los cambios en el contexto de manera segura.
        /// </summary>
        protected async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar cambios en la base de datos: {ex.Message}");
                throw;
            }
        }
    }
}
