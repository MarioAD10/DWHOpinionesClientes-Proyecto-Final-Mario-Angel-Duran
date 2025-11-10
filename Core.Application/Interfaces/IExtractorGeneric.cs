namespace Core.Application.Interfaces
{
    /// <summary>
    /// Interfaz genérica para extractores de datos que devuelven registros crudos.
    /// </summary>
    public interface IExtractorGeneric<T>
    {

        /// <summary>
        /// Ejecuta la extracción y devuelve una colección de datos del tipo especificado.
        /// </summary>
        /// <returns>Enumeración de registros extraídos.</returns>
        Task<IEnumerable<T>> ExtractAsync();
    }
}
