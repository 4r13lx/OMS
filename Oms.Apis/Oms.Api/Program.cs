using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Polly;
using Polly.Extensions.Http;
using Oms.Api.Infrastructure;
using Oms.Application.Services;
using Oms.Core.Interfaces;
using Oms.Data;
using Oms.Data.Repositories;
using Oms.Infrastructure.Contracts.ExternalPriceMock;
using Oms.Infrastructure.Services.ExternalPriceMock;

namespace Oms.Api;

/// <summary>
/// Clase de entrada de la aplicación.
/// Aquí se configura el contenedor de Dependencias (DI), el Pipeline de Middleware y las políticas de Resiliencia.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuración de la URL de escucha (Comentada para permitir configuración vía variables de entorno y docker-compose)
        //builder.WebHost.UseUrls("http://localhost:5000");

        // --- REGISTRO DE SERVICIOS (Contenedor de Inyección de Dependencias) ---

        // Manejo de errores centralizado con Problem Details (estándar RFC 7807)
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configuración de la Base de Datos (Entity Framework Core con SQLite)
        builder.Services.AddDbContext<OmsDbContext>(options => 
            options.UseSqlite(builder.Configuration.GetValue<string>("ConnectionStrings:OmsDb") ?? "Data Source=oms.db"));

        // Registro de Repositorios (Patrón Repository para desacoplar la persistencia)
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IFinancialAssetRepository, FinancialAssetRepository>();
        
        // Registro de Servicios de Aplicación
        builder.Services.AddScoped<IOrderService, OrderService>();

        // Configuración de FluentValidation (Escaneo automático de validadores en el proyecto de Application)
        builder.Services.AddValidatorsFromAssembly(typeof(Oms.Application.Validators.OrderCreateRequestValidator).Assembly);

        // Configuración de MediatR (Patrón Mediator y Pipeline Behaviors)
        builder.Services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(Oms.Application.Orders.Commands.CreateOrderCommand).Assembly);
            
            // Registro de Comportamientos Transversales (Validación automática)
            cfg.AddOpenBehavior(typeof(Oms.Application.Common.Behaviors.ValidationBehavior<,>));
        });

        Console.WriteLine(
            $"PriceMockApi BaseUrl: {builder.Configuration["PriceMockApi:BaseUrl"]}"
        );

        foreach (var kv in builder.Configuration.AsEnumerable())
        {
            Console.WriteLine($"{kv.Key} = {kv.Value}");
        }

        // Configuración de HttpClient con Políticas de Resiliencia (Polly)
        builder.Services.AddHttpClient<IExternalPriceMockProvider, ExternalPriceMockProvider>(client =>        {
            var baseUrl = builder.Configuration.GetValue<string>("PriceMockApi:BaseUrl") ?? "http://localhost:5001/";
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddPolicyHandler(HttpPolicyExtensions
            // 1. Maneja errores de red y timeouts
            .HandleTransientHttpError()
            // 2. Maneja específicamente el error 503 (Servicio No Disponible) que el Mock lanza a propósito
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            // 3. Aplica política de Reintento con Espera Exponencial (Retries: 3, Espera: 2^n segundos)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        var app = builder.Build();

        // --- CONFIGURACIÓN DEL PIPELINE DE MIDDLEWARE (Orden de ejecución de peticiones) ---

        // Aseguramos que la base de datos esté creada y con datos semilla al arrancar
        app.Services.EnsureDatabaseCreated();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Activación del Middleware de manejo global de excepciones
        app.UseExceptionHandler();

        app.MapControllers();
        app.Run();
    }
}
