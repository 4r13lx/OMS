using Microsoft.EntityFrameworkCore;
using Oms.Core.Entities;
using Oms.Core.Interfaces;

namespace Oms.Data.Repositories
{
    /// <summary>
    /// Implementación del patrón Repository para la entidad FinancialAsset.
    /// El patrón Repository abstrae la lógica de acceso a datos, proporcionando una interfaz limpia
    /// para el dominio y permitiendo desacoplar la lógica de negocio de la infraestructura de persistencia.
    /// </summary>
    public sealed class FinancialAssetRepository : IFinancialAssetRepository
    {
        private readonly OmsDbContext _dbContext;

        public FinancialAssetRepository(OmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FinancialAsset?> GetByTickerAsync(string ticker)
        {
            return await _dbContext.FinancialAssets.AsNoTracking()
                .FirstOrDefaultAsync(asset => asset.Ticker == ticker);
        }

        public async Task<IReadOnlyList<FinancialAsset>> GetAllAsync()
        {
            return await _dbContext.FinancialAssets.AsNoTracking().OrderBy(asset => asset.Ticker).ToListAsync();
        }
    }
}
