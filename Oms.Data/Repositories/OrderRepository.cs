using Microsoft.EntityFrameworkCore;
using Oms.Core.Entities;
using Oms.Core.Interfaces;

namespace Oms.Data.Repositories
{
    /// <summary>
    /// Implementación concreta del repositorio de Órdenes usando EF Core.
    /// El patrón Repository actúa como un mediador entre la capa de dominio y el mapeo de datos,
    /// proporcionando una interfaz similar a una colección de objetos en memoria.
    /// </summary>
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly OmsDbContext _dbContext;

        public OrderRepository(OmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Busca una orden por su identificador único.
        /// </summary>
        public async Task<InvestmentOrder?> GetByIdAsync(int id)
        {
            return await _dbContext.Orders.FindAsync(id);
        }

        /// <summary>
        /// Obtiene todas las órdenes registradas.
        /// Se usa AsNoTracking() para mejorar el rendimiento en operaciones de solo lectura.
        /// </summary>
        public async Task<IReadOnlyList<InvestmentOrder>> GetAllAsync()
        {
            return await _dbContext.Orders.AsNoTracking().OrderBy(o => o.Id).ToListAsync();
        }

        /// <summary>
        /// Consulta especializada para obtener órdenes de una cuenta específica.
        /// </summary>
        public async Task<IReadOnlyList<InvestmentOrder>> GetByAccountIdAsync(int accountId)
        {
            return await _dbContext.Orders.AsNoTracking().Where(o => o.AccountId == accountId).OrderBy(o => o.Id).ToListAsync();
        }

        /// <summary>
        /// Agrega una nueva orden y persiste los cambios.
        /// </summary>
        public async Task<InvestmentOrder> AddAsync(InvestmentOrder order)
        {
            var entry = await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
            return entry.Entity;
        }

        /// <summary>
        /// Actualiza el estado de una orden existente.
        /// </summary>
        public async Task UpdateAsync(InvestmentOrder order)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina físicamente una orden del sistema.
        /// </summary>
        public async Task DeleteAsync(InvestmentOrder order)
        {
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Verifica la existencia de una orden sin cargarla en memoria.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Orders.AnyAsync(o => o.Id == id);
        }
    }
}
