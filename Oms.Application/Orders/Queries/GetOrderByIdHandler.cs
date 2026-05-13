using MediatR;
using Oms.Application.Dtos;
using Oms.Core.Exceptions;
using Oms.Core.Interfaces;
using Oms.Core.Entities;

namespace Oms.Application.Orders.Queries
{
    /// <summary>
    /// Handler para la consulta GetOrderByIdQuery.
    /// Se encarga exclusivamente de la lógica de lectura y mapeo.
    /// </summary>
    public sealed class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            // Buscamos la entidad en el repositorio
            var order = await _orderRepository.GetByIdAsync(request.Id);

            // Si no existe, lanzamos una excepción de dominio que el middleware convertirá en 404
            if (order is null)
            {
                throw new NotFoundException($"Orden con id {request.Id} no encontrada.");
            }

            // Mapeamos manualmente a DTO (en un proyecto real se podría usar AutoMapper)
            return MapToResponse(order);
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
