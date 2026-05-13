using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Dtos;
using Oms.Application.Services;
using Oms.Application.Validators;
using Oms.Core.Enums;
using Oms.Data;
using Oms.Data.Repositories;
using Oms.Infrastructure.Contracts.ExternalPriceMock;
using Xunit;

namespace Oms.Test;

/// <summary>
/// Pruebas de unidad para el servicio de órdenes.
/// Se utiliza una base de datos en memoria (In-Memory Database) para garantizar que las pruebas sean rápidas
/// y estén aisladas entre sí, evitando la necesidad de una infraestructura de base de datos real.
/// </summary>
public sealed class OrderServiceTests
{
    private readonly IValidator<OrderCreateRequest> _validator = new OrderCreateRequestValidator();

    /// <summary>
    /// Verifica la regla de negocio para Fondos Comunes de Inversión (FCI).
    /// En este caso, el cálculo del monto total no debe incluir comisiones ni impuestos.
    /// </summary>
    [Fact]
    public async Task CreateFciOrder_CalculatesAmountWithoutFees()
    {
        using var context = CreateContext("fci-test-db");
        await SeedAssets(context);

        var orderRepository = new OrderRepository(context);
        var assetRepository = new FinancialAssetRepository(context);
        var externalPriceProvider = new StubPriceProvider();
        var orderService = new OrderService(orderRepository, assetRepository, externalPriceProvider, _validator);

        var request = new OrderCreateRequest
        {
            AccountId = 1,
            Ticker = "Delta.Pesos",
            Quantity = 1000,
            Price = 0.0181m,
            Operation = 'C'
        };

        var response = await orderService.CreateOrderAsync(request);

        Assert.Equal(18.1m, response.TotalAmount);
        Assert.Equal(0m, response.CommissionAmount);
        Assert.Equal(0m, response.TaxAmount);
        Assert.Equal(OrderStatus.EnProceso, response.Status);
    }

    /// <summary>
    /// Verifica la regla de negocio para Acciones.
    /// A diferencia de los FCI, las órdenes de acciones deben utilizar un precio externo
    /// y aplicar las comisiones e impuestos correspondientes sobre el monto total.
    /// </summary>
    [Fact]
    public async Task CreateAccionOrder_UsesExternalPriceAndAppliesFees()
    {
        using var context = CreateContext("accion-test-db");
        await SeedAssets(context);

        var orderRepository = new OrderRepository(context);
        var assetRepository = new FinancialAssetRepository(context);
        var externalPriceProvider = new StubPriceProvider(200m);
        var orderService = new OrderService(orderRepository, assetRepository, externalPriceProvider, _validator);

        var request = new OrderCreateRequest
        {
            AccountId = 2,
            Ticker = "AAPL",
            Quantity = 10,
            Operation = 'V'
        };

        var response = await orderService.CreateOrderAsync(request);

        Assert.Equal(2000m, response.TotalAmount);
        Assert.Equal(12m, response.CommissionAmount);
        Assert.Equal(2.52m, response.TaxAmount);
        Assert.Equal('V', response.Operation);
    }

    /// <summary>
    /// Valida que el cambio de estado de una orden se realice correctamente sin afectar otros atributos.
    /// </summary>
    [Fact]
    public async Task UpdateOrderStatus_OnlyStatusChanged()
    {
        using var context = CreateContext("status-test-db");
        await SeedAssets(context);

        var orderRepository = new OrderRepository(context);
        var assetRepository = new FinancialAssetRepository(context);
        var externalPriceProvider = new StubPriceProvider(100m);
        var orderService = new OrderService(orderRepository, assetRepository, externalPriceProvider, _validator);

        var createRequest = new OrderCreateRequest
        {
            AccountId = 3,
            Ticker = "AAPL",
            Quantity = 1,
            Operation = 'C'
        };

        var created = await orderService.CreateOrderAsync(createRequest);
        await orderService.UpdateOrderStatusAsync(created.Id, new OrderUpdateRequest { Status = OrderStatus.Ejecutada });

        var updated = await orderService.GetOrderByIdAsync(created.Id);
        Assert.Equal(OrderStatus.Ejecutada, updated.Status);
    }

    /// <summary>
    /// Configura y retorna un contexto de base de datos utilizando el proveedor en memoria.
    /// Esto permite simular operaciones de persistencia de forma eficiente durante las pruebas.
    /// </summary>
    private static OmsDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<OmsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new OmsDbContext(options);
    }

    /// <summary>
    /// Prepara los datos iniciales necesarios (activos financieros) en la base de datos en memoria
    /// para que las pruebas tengan un estado consistente.
    /// </summary>
    private static async Task SeedAssets(OmsDbContext context)
    {
        if (!await context.FinancialAssets.AnyAsync())
        {
            context.FinancialAssets.AddRange(
                new Oms.Core.Entities.FinancialAsset { Id = 1, Ticker = "AAPL", Name = "Apple", AssetType = AssetType.Accion, BasePrice = 177.97m },
                new Oms.Core.Entities.FinancialAsset { Id = 8, Ticker = "Delta.Pesos", Name = "Delta Pesos Clase A", AssetType = AssetType.FCI, BasePrice = 0.0181m }
            );
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Implementación de 'Stub' para el proveedor de precios externos.
    /// El uso de Stubs o Mocks es esencial para aislar la lógica de negocio de dependencias externas
    /// (como una API de mercado), permitiendo predecir el comportamiento y asegurar que el test
    /// solo evalúe el código del servicio.
    /// </summary>
    private sealed class StubPriceProvider : IExternalPriceMockProvider
    {
        private readonly decimal _price;

        public StubPriceProvider(decimal price = 100m)
        {
            _price = price;
        }

        public Task<decimal> GetPriceAsync(string ticker)
        {
            return Task.FromResult(_price);
        }
    }
}
