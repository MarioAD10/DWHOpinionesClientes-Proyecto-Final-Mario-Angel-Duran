namespace Core.Application.Interfaces.ILoaders
{
    // <summary>
    /// Interfaz genérica para loaders de dimensiones.
    /// Define el contrato básico que todos los loaders deben cumplir.
    /// </summary>
    /// <typeparam name="TSource">Tipo de DTO de origen</typeparam>
    /// <typeparam name="TDimension">Tipo de entidad de dimensión</typeparam>
    public interface IDimensionLoader<TSource, TDimension>
    {
        /// <summary>
        /// Carga múltiples registros en la dimensión.
        /// </summary>
        /// <param name="sourceRecords">Colección de datos de origen</param>
        /// <returns>Cantidad de registros procesados (insertados o actualizados)</returns>
        Task<int> LoadAsync(IEnumerable<TSource> sourceRecords);

        /// <summary>
        /// Obtiene la clave surrogate de un registro.
        /// Si no existe, lo crea y retorna la nueva clave.
        /// </summary>
        /// <param name="sourceRecord">Datos del registro</param>
        /// <returns>La clave surrogate (Key) del registro</returns>
        Task<int> GetOrCreateKeyAsync(TSource sourceRecord);
    }
}
