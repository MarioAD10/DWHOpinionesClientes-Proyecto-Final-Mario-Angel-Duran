using Core.Application.Models;

namespace Core.Application.Interfaces
{
    public interface IExtractor
    {
        string SourceName { get; }
        /// <summary>
        /// Ejecuta la extracción de datos desde la fuente especificada.
        /// </summary>
        /// <param name="parameters">Parámetros de configuración de la extracción.</param>
        /// <returns>Resultado con estadísticas y estado de la operación.</returns>
        /// 

        Task<ExtractionResult> ExtractAsync(ExtractionParams parameters);
    }
}
