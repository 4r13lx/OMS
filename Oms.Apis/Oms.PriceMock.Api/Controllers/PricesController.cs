using Microsoft.AspNetCore.Mvc;
using Oms.PriceMock.Application.Contracts;

namespace Oms.PriceMock.Api.Controllers
{
    /// <summary>
    /// Controlador que simula un servicio externo de precios (External Service Simulator).
    /// Este proyecto es fundamental para las pruebas de integración, ya que permite desacoplar
    /// el sistema principal de proveedores reales de datos de mercado.
    /// Además, este simulador está diseñado para introducir fallos aleatorios y latencias,
    /// permitiendo probar la resiliencia del sistema principal mediante el uso de librerías como Polly.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PricesController : ControllerBase
    {
        private readonly IPriceMockService _priceMockService;

        public PricesController(IPriceMockService priceMockService)
        {
            _priceMockService = priceMockService;
        }

        /// <summary>
        /// Obtiene la lista completa de activos y sus precios simulados.
        /// Este endpoint puede retornar errores aleatorios para simular inestabilidad en la red
        /// o en el servicio externo, obligando al cliente a implementar estrategias de reintento.
        /// </summary>
        /// <returns>Una colección de activos con precios generados dinámicamente.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var price = await _priceMockService.GetAssetsAsync();
            return Ok(price);
        }
        
        /// <summary>
        /// Obtiene el precio simulado de un activo específico por su ticker.
        /// Útil para verificar cómo el sistema principal maneja respuestas individuales de un servicio externo.
        /// </summary>
        /// <param name="ticker">El símbolo del activo (ej. AAPL, MSFT).</param>
        /// <returns>Los detalles de precio para el activo solicitado.</returns>
        [HttpGet("{ticker}")]
        public async Task<IActionResult> GetPrice(string ticker)
        {
            var price = await _priceMockService.GetAssetsByTickerAsync(ticker);
            return Ok(price);
        }
    }
}
