using Microsoft.EntityFrameworkCore;
using Oms.Core.Entities;
using Oms.Core.Enums;

namespace Oms.Data
{
    /// <summary>
    /// El DbContext es la puerta de enlace principal para interactuar con la base de datos a través de Entity Framework Core.
    /// Actúa como una combinación de los patrones Unit of Work y Repository, gestionando la conexión, 
    /// el seguimiento de cambios y la persistencia de los datos.
    /// </summary>
    public sealed class OmsDbContext : DbContext
    {
        public OmsDbContext(DbContextOptions<OmsDbContext> options)
            : base(options)
        {
        }

        public DbSet<InvestmentOrder> Orders => Set<InvestmentOrder>();
        public DbSet<FinancialAsset> FinancialAssets => Set<FinancialAsset>();

        /// <summary>
        /// Configura el modelo de datos y las relaciones entre entidades.
        /// Aquí se utiliza 'Seed Data' para pre-poblar la base de datos con activos financieros iniciales,
        /// lo cual es útil para entornos de desarrollo y pruebas.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvestmentOrder>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.AssetName).HasMaxLength(32).IsRequired();
                entity.Property(x => x.Operation).HasConversion<string>().HasMaxLength(1).IsRequired();
                entity.Property(x => x.Price).HasPrecision(18, 4);
                entity.Property(x => x.TotalAmount).HasPrecision(18, 4);
                entity.Property(x => x.CommissionAmount).HasPrecision(18, 4);
                entity.Property(x => x.TaxAmount).HasPrecision(18, 4);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<FinancialAsset>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Ticker).HasMaxLength(16).IsRequired();
                entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
                entity.Property(x => x.BasePrice).HasPrecision(18, 4);
            });

            modelBuilder.Entity<FinancialAsset>().HasData(
                new FinancialAsset { Id = 1, Ticker = "AAPL", Name = "Apple", AssetType = AssetType.Accion, BasePrice = 177.97m },
                new FinancialAsset { Id = 2, Ticker = "GOOGL", Name = "Alphabet Inc", AssetType = AssetType.Accion, BasePrice = 138.21m },
                new FinancialAsset { Id = 3, Ticker = "MSFT", Name = "Microsoft", AssetType = AssetType.Accion, BasePrice = 329.04m },
                new FinancialAsset { Id = 4, Ticker = "KO", Name = "Coca Cola", AssetType = AssetType.Accion, BasePrice = 58.3m },
                new FinancialAsset { Id = 5, Ticker = "WMT", Name = "Walmart", AssetType = AssetType.Accion, BasePrice = 163.42m },
                new FinancialAsset { Id = 6, Ticker = "AL30", Name = "BONOS ARGENTINA USD 2030 L.A", AssetType = AssetType.Bono, BasePrice = 307.4m },
                new FinancialAsset { Id = 7, Ticker = "GD30", Name = "Bonos Globales Arg. USD Step Up 2030", AssetType = AssetType.Bono, BasePrice = 336.1m },
                new FinancialAsset { Id = 8, Ticker = "Delta.Pesos", Name = "Delta Pesos Clase A", AssetType = AssetType.FCI, BasePrice = 0.0181m },
                new FinancialAsset { Id = 9, Ticker = "Fima.Premium", Name = "Fima Premium Clase A", AssetType = AssetType.FCI, BasePrice = 0.0317m }
            );
        }
    }
}
