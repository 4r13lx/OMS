using Oms.Core.Entities;

namespace Oms.Core.Interfaces
{
    /// <summary>
    /// Define el contrato para la persistencia y recuperación de órdenes de inversión.
    /// </summary>
    /// <remarks>
    /// El patrón Repository actúa como una abstracción sobre la capa de acceso a datos. 
    /// Define un "contrato" que la infraestructura debe implementar, permitiendo que el 
    /// dominio permanezca agnóstico de si los datos provienen de una base de datos SQL, NoSQL o una API.
    /// </remarks>
    public interface IOrderRepository
    {
        /// <summary>
        /// Obtiene una orden por su identificador único.
        /// </summary>
        Task<InvestmentOrder?> GetByIdAsync(int id);

        /// <summary>
        /// Recupera todas las órdenes registradas.
        /// </summary>
        Task<IReadOnlyList<InvestmentOrder>> GetAllAsync();

        /// <summary>
        /// Obtiene las órdenes asociadas a una cuenta específica.
        /// </summary>
        Task<IReadOnlyList<InvestmentOrder>> GetByAccountIdAsync(int accountId);

        /// <summary>
        /// Registra una nueva orden en el sistema.
        /// </summary>
        Task<InvestmentOrder> AddAsync(InvestmentOrder order);

        /// <summary>
        /// Actualiza el estado o datos de una orden existente.
        /// </summary>
        Task UpdateAsync(InvestmentOrder order);

        /// <summary>
        /// Elimina una orden del sistema.
        /// </summary>
        Task DeleteAsync(InvestmentOrder order);

        /// <summary>
        /// Verifica la existencia de una orden.
        /// </summary>
        Task<bool> ExistsAsync(int id);
    }
}
