using Oms.Core.Entities;

namespace Oms.Core.Interfaces
{
    /// <summary>
    /// Contrato para el acceso a datos de activos financieros.
    /// </summary>
    /// <remarks>
    /// Al igual que otros repositorios, esta interfaz asegura que el núcleo de la aplicación
    /// no dependa de implementaciones específicas de almacenamiento.
    /// </remarks>
    public interface IFinancialAssetRepository
    {
        /// <summary>
        /// Busca un activo financiero utilizando su símbolo (Ticker).
        /// </summary>
        /// <param name="ticker">El símbolo de cotización a buscar.</param>
        /// <returns>El activo si se encuentra; de lo contrario, null.</returns>
        Task<FinancialAsset?> GetByTickerAsync(string ticker);

        /// <summary>
        /// Obtiene el listado completo de activos financieros disponibles.
        /// </summary>
        Task<IReadOnlyList<FinancialAsset>> GetAllAsync();
    }
}
