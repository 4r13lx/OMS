using MediatR;
using Microsoft.AspNetCore.Mvc;
using Oms.Application.Dtos;
using Oms.Application.Orders.Commands;
using Oms.Application.Orders.Queries;
using Oms.Application.Services;
using Oms.Core.Exceptions;

namespace Oms.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestión de órdenes de inversión.
    /// En una arquitectura limpia, el controlador es "delgado" (thin controller),
    /// delegando la ejecución del negocio a MediatR.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMediator _mediator;

        public OrdersController(IOrderService orderService, IMediator mediator)
        {
            _orderService = orderService;
            _mediator = mediator;
        }

        /// <summary>
        /// Crea una nueva orden de inversión.
        /// Utiliza el comando CreateOrderCommand para enviar la petición a través del bus de MediatR.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] OrderCreateRequest request)
        {
            // El controlador no sabe CÓMO se crea una orden, solo envía la petición.
            var order = await _mediator.Send(new CreateOrderCommand(request));
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        /// <summary>
        /// Obtiene todas las órdenes registradas.
        /// Todavía utiliza el servicio directamente (pendiente de migrar a Queries).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetAll()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Obtiene una orden específica por su ID.
        /// Ejemplo de implementación de una "Query" usando MediatR.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponse>> GetById(int id)
        {
            // Enviamos la Query al bus. El controlador no sabe que el Handler consulta la DB.
            var order = await _mediator.Send(new GetOrderByIdQuery(id));
            return Ok(order);
        }

        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderUpdateRequest request)
        {
            await _orderService.UpdateOrderStatusAsync(id, request);
            return NoContent();
        }

        /// <summary>
        /// Elimina una orden del sistema.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
