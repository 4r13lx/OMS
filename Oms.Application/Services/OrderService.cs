using FluentValidation;
using Oms.Application.Dtos;
using Oms.Core.Entities;
using Oms.Core.Enums;
using Oms.Core.Exceptions;
using Oms.Core.Interfaces;
using Oms.Infrastructure.Contracts.ExternalPriceMock;

namespace Oms.Application.Services
{
    /// <summary>
    /// Implementación del servicio de órdenes que orquestra la lógica de negocio.
    /// </summary>
    /// <remarks>
    /// Este servicio es un componente central de la **Capa de Aplicación**. 
    /// Implementa el patrón "Transaction Script" o "Service Layer", coordinando el acceso a repositorios,
    /// validaciones y servicios externos. 
    /// Como parte de la evolución hacia **CQRS**, este servicio demuestra cómo la lógica compleja
    /// puede centralizarse antes de ser refactorizada en Handlers más granulares.
    /// </remarks>
    public sealed class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFinancialAssetRepository _assetRepository;
        private readonly IExternalPriceMockProvider _priceProvider;
        private readonly IValidator<OrderCreateRequest> _createRequestValidator;

        public OrderService(
            IOrderRepository orderRepository,
            IFinancialAssetRepository assetRepository,
            IExternalPriceMockProvider priceProvider,
            IValidator<OrderCreateRequest> createRequestValidator)
        {
            _orderRepository = orderRepository;
            _assetRepository = assetRepository;
            _priceProvider = priceProvider;
            _createRequestValidator = createRequestValidator;
        }

        public async Task<OrderResponse> CreateOrderAsync(OrderCreateRequest request)
        {
            // 1. Validación de la entrada usando FluentValidation
            var validationResult = await _createRequestValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new DomainException(validationResult.Errors.First().ErrorMessage);
            }

            // 2. Verificación de la existencia del activo en el dominio
            var ticker = request.Ticker.Trim();
            var asset = await _assetRepository.GetByTickerAsync(ticker);
            if (asset is null)
            {
                throw new NotFoundException($"Activo financiero no encontrado: {ticker}");
            }

            // 3. Preparación de datos y resolución de precio (regla de negocio: acciones vs bonos/fci)
            var operation = ParseOperation(request.Operation);
            var price = await ResolvePriceAsync(asset, request.Price);

            // 4. Cálculos financieros (Estrategia basada en el tipo de activo)
            var totalAmount = price * request.Quantity;
            var commission = ComputeCommission(asset.AssetType, totalAmount);
            var tax = ComputeTax(asset.AssetType, commission);

            // 5. Mapeo de DTO a Entidad de Dominio
            var order = new InvestmentOrder
            {
                AccountId = request.AccountId,
                Ticker = ticker,
                AssetName = asset.Name,
                AssetType = asset.AssetType,
                Quantity = request.Quantity,
                Price = price,
                Operation = operation,
                TotalAmount = totalAmount,
                CommissionAmount = commission,
                TaxAmount = tax,
                CreatedAt = DateTime.UtcNow
            };

            // 6. Persistencia a través del Repositorio (Capa de Infraestructura)
            var created = await _orderRepository.AddAsync(order);

            // 7. Retorno de DTO de respuesta para desacoplar la API
            return MapToResponse(created);
        }

        public async Task<IReadOnlyList<OrderResponse>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<OrderResponse> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
            {
                throw new NotFoundException($"Orden con id {id} no encontrada.");
            }

            return MapToResponse(order);
        }

        public async Task UpdateOrderStatusAsync(int id, OrderUpdateRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
            {
                throw new NotFoundException($"Orden con id {id} no encontrada.");
            }

            // Aplicamos lógica de cambio de estado definida en la entidad de dominio
            order.SetStatus(request.Status);
            await _orderRepository.UpdateAsync(order);
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
            {
                throw new NotFoundException($"Orden con id {id} no encontrada.");
            }

            await _orderRepository.DeleteAsync(order);
        }

        public async Task<AccountSummaryResponse> GetAccountSummaryAsync(int accountId)
        {
            if (accountId <= 0)
            {
                throw new DomainException("El AccountId debe ser un número positivo.");
            }

            // Recuperación y agregación de datos para el reporte de cuenta
            var orders = await _orderRepository.GetByAccountIdAsync(accountId);
            var byStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(group => group.Key.ToString(), group => group.Count());

            return new AccountSummaryResponse
            {
                AccountId = accountId,
                TotalOrders = orders.Count,
                TotalNotionalAmount = orders.Sum(o => o.TotalAmount),
                TotalCommissionAmount = orders.Sum(o => o.CommissionAmount),
                TotalTaxAmount = orders.Sum(o => o.TaxAmount),
                OrdersByStatus = byStatus
            };
        }

        /// <summary>
        /// Normaliza y valida el carácter de operación.
        /// </summary>
        private static char ParseOperation(char operation)
        {
            var normalized = char.ToUpperInvariant(operation);
            if (normalized == 'C' || normalized == 'V')
            {
                return normalized;
            }

            throw new DomainException("La operación debe ser 'C' o 'V'.");
        }

        /// <summary>
        /// Resuelve el precio según el tipo de activo. 
        /// Las acciones usan un proveedor externo, mientras que Bonos/FCI requieren precio manual.
        /// </summary>
        private async Task<decimal> ResolvePriceAsync(FinancialAsset asset, decimal? requestedPrice)
        {
            return asset.AssetType switch
            {
                AssetType.Accion => await _priceProvider.GetPriceAsync(asset.Ticker),
                AssetType.Bono => requestedPrice.HasValue && requestedPrice.Value > 0
                    ? requestedPrice.Value
                    : throw new DomainException("El precio es obligatorio para bonos y debe ser mayor que cero."),
                AssetType.FCI => requestedPrice.HasValue && requestedPrice.Value > 0
                    ? requestedPrice.Value
                    : throw new DomainException("El precio es obligatorio para FCI y debe ser mayor que cero."),
                _ => throw new DomainException("Tipo de activo no válido para la orden.")
            };
        }

        /// <summary>
        /// Calcula la comisión del broker basada en el tipo de activo y el monto operado.
        /// </summary>
        private static decimal ComputeCommission(AssetType assetType, decimal totalAmount)
        {
            return assetType switch
            {
                AssetType.Accion => decimal.Round(totalAmount * 0.006m, 4), // 0.6% para Acciones
                AssetType.Bono => decimal.Round(totalAmount * 0.002m, 4),   // 0.2% para Bonos
                AssetType.FCI => 0m,                                        // FCI sin comisión directa
                _ => 0m
            };
        }

        /// <summary>
        /// Calcula los impuestos aplicables (ej: IVA sobre la comisión).
        /// </summary>
        private static decimal ComputeTax(AssetType assetType, decimal commission)
        {
            return assetType switch
            {
                AssetType.Accion => decimal.Round(commission * 0.21m, 4), // IVA 21% sobre comisión
                AssetType.Bono => decimal.Round(commission * 0.21m, 4),
                AssetType.FCI => 0m,
                _ => 0m
            };
        }

        /// <summary>
        /// Realiza el mapeo manual de la Entidad al DTO de respuesta.
        /// En proyectos más grandes, se podría usar AutoMapper.
        /// </summary>
        private static OrderResponse MapToResponse(InvestmentOrder order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                AccountId = order.AccountId,
                Ticker = order.Ticker,
                AssetName = order.AssetName,
                AssetType = order.AssetType,
                Quantity = order.Quantity,
                Price = order.Price,
                Operation = order.Operation,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CommissionAmount = order.CommissionAmount,
                TaxAmount = order.TaxAmount,
                CreatedAt = order.CreatedAt
            };
        }
    }
}
