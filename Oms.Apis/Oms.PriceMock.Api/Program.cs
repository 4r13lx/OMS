using Oms.PriceMock.Application.Contracts;
using Oms.PriceMock.Application.Repositories;
using Oms.PriceMock.Application.Services.Services;
using Oms.PriceMock.Application.Simulation.NetworkIssues;
using Oms.PriceMock.Api.Infrastructure;

namespace Oms.PriceMock.Api;

/// <summary>
/// Punto de entrada principal para el simulador de precios (PriceMock API).
/// Este proyecto funciona como un 'External Service Simulator', permitiendo que el equipo de desarrollo
/// trabaje de forma independiente sin depender de APIs de terceros costosas o inestables durante la fase de desarrollo.
/// Facilita las pruebas de integración y el desacoplamiento arquitectónico.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        // Configuración del Host y Servicios
        var builder = WebApplication.CreateBuilder(args);

        // Configuración de la URL de escucha (Comentada para permitir configuración vía variables de entorno y docker-compose)
        //builder.WebHost.UseUrls("http://localhost:5001");

        // Registro de servicios en el contenedor de Inyección de Dependencias (DI)
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<AssetStore>();
        builder.Services.AddSingleton<IAssetStore, AssetStore>();

        // Registro del servicio de precios con un decorador para manejo de límites de tasa (Rate Limiting)
        builder.Services.AddScoped<IPriceMockService>(sp =>
        {
            var provider = sp.GetRequiredService<PriceMockService>();
            return new RateLimitDecorator(provider);
        });

        // Configuración de HttpClient para el servicio de precios (aunque en este caso no se consume un API externo real, se mantiene la estructura para futuras integraciones)
        builder.Services.AddHttpClient<PriceMockService>(client =>
        {
            var baseUrl = builder.Configuration.GetValue<string>("PriceMockApi:BaseUrl") ?? "http://localhost:5001/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Construcción de la aplicación y configuración del Pipeline de Middleware
        var app = builder.Build();

        // Configuración del Middleware para desarrollo (Swagger) y producción (manejo de errores, HTTPS, etc.)
        if (builder.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Configuración del Middleware para manejo de errores, redirección HTTPS, autorización, etc.
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseExceptionHandler();
        app.MapControllers();
        app.Run();
    }
}
