using MediatR;
using Oms.Application.Dtos;
using Oms.Core.Entities;
using Oms.Core.Enums;
using Oms.Core.Exceptions;
using Oms.Core.Interfaces;
using Oms.Application.Services;
using Oms.Infrastructure.Contracts.ExternalPriceMock;

namespace Oms.Application.Orders.Commands
{
    /// <summary>
    /// Handler para el comando de creación de órdenes.
    /// Implementa el patrón Mediator para separar la lógica de negocio de los controladores.
    /// Esta clase orquesta la creación, interactuando con repositorios y servicios externos.
    /// </summary>
    public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFinancialAssetRepository _assetRepository;
        private readonly IExternalPriceMockProvider _priceProvider;

        public CreateOrderHandler(
            IOrderRepository orderRepository,
            IFinancialAssetRepository assetRepository,
            IExternalPriceMockProvider priceProvider)
        {
            _orderRepository = orderRepository;
            _assetRepository = assetRepository;
            _priceProvider = priceProvider;
        }

        /// <summary>
        /// Punto de entrada del caso de uso.
        /// Nota: No hay validación explícita aquí porque la maneja el ValidationBehavior.
        /// </summary>
        public async Task<OrderResponse> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // 1. Obtención del activo para validar existencia y tipo.
            var ticker = request.Ticker.Trim();
            var asset = await _assetRepository.GetByTickerAsync(ticker);
            if (asset is null)
            {
                throw new NotFoundException($"Activo financiero no encontrado: {ticker}");
            }

            // 2. Resolución de precios según reglas de negocio por tipo de activo.
            var operation = char.ToUpperInvariant(request.Operation);
            var price = await ResolvePriceAsync(asset, request.Price);

            // 3. Cálculos financieros (Comisiones e Impuestos).
            var totalAmount = price * request.Quantity;
            var commission = ComputeCommission(asset.AssetType, totalAmount);
            var tax = ComputeTax(asset.AssetType, commission);

            // 4. Mapeo a entidad de dominio y persistencia.
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

            var created = await _orderRepository.AddAsync(order);
            
            // 5. Mapeo a DTO de respuesta para no exponer la entidad de dominio.
            return MapToResponse(created);
        }

        /// <summary>
        /// Lógica compleja: Determina si se usa el precio solicitado o se consulta al mercado.
        /// - Acciones: Requieren precio de mercado externo.
        /// - Bonos/FCI: Requieren precio manual proporcionado por el usuario.
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
        /// Cálculo de comisiones basado en el tipo de activo.
        /// Acciones: 0.6%, Bonos: 0.2%, FCI: Sin comisión.
        /// </summary>
        private static decimal ComputeCommission(AssetType assetType, decimal totalAmount)
        {
            return assetType switch
            {
                AssetType.Accion => decimal.Round(totalAmount * 0.006m, 4),
                AssetType.Bono => decimal.Round(totalAmount * 0.002m, 4),
                AssetType.FCI => 0m,
                _ => 0m
            };
        }

        /// <summary>
        /// Cálculo de impuestos (IVA del 21% sobre la comisión).
        /// </summary>
        private static decimal ComputeTax(AssetType assetType, decimal commission)
        {
            return assetType switch
            {
                AssetType.Accion => decimal.Round(commission * 0.21m, 4),
                AssetType.Bono => decimal.Round(commission * 0.21m, 4),
                AssetType.FCI => 0m,
                _ => 0m
            };
        }

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
