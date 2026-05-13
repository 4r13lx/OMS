using Microsoft.AspNetCore.Mvc;
using Oms.Application.Services;

namespace Oms.Api.Controllers
{
    /// <summary>
    /// Controlador encargado de gestionar las operaciones relacionadas con las cuentas de usuario.
    /// Este controlador actúa como el punto de entrada para obtener información agregada y resumida de las cuentas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AccountsController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AccountsController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Obtiene un resumen detallado de la cuenta especificada.
        /// Este endpoint delega la lógica de negocio al servicio de órdenes para consolidar
        /// datos como el saldo, las posiciones abiertas y el rendimiento histórico.
        /// Es un ejemplo de cómo centralizar la lógica de resumen de datos para simplificar la carga en el cliente.
        /// </summary>
        /// <param name="accountId">Identificador único de la cuenta.</param>
        /// <returns>Un objeto con el resumen consolidado de la cuenta.</returns>
        [HttpGet("{accountId:int}/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> GetSummary(int accountId)
        {
            var summary = await _orderService.GetAccountSummaryAsync(accountId);
            return Ok(summary);
        }
    }
}
