# Guía de Implementación

## 🔧 Stack Tecnológico

| Componente | Versión | Propósito |
|-----------|---------|----------|
| .NET | 8.0 | Framework principal |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0.15 | ORM para acceso a datos |
| SQLite | Incluido | Base de datos embebida |
| Swashbuckle | 6.6.2 | Generación de Swagger/OpenAPI |
| xUnit | Estándar | Framework de testing |

## 🏗️ Patrones de Implementación

### 1. Patrón Mediator (MediatR)

**Propósito:** Desacoplar los controladores de los casos de uso.

**Implementación (Command):**
```csharp
// Comando inmutable
public sealed record CreateOrderCommand(OrderCreateRequest Request) : IRequest<OrderResponse>;

// Handler con lógica de orquestación
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    // Inyección de dependencias necesaria
    public async Task<OrderResponse> Handle(CreateOrderCommand command, CancellationToken ct) 
    {
        // Lógica del caso de uso...
    }
}
```

### 2. Pipeline Behaviors (Validación Automática)

**Propósito:** Validar todos los comandos antes de que lleguen a su Handler.

```csharp
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, next, ct)
    {
        // Ejecuta FluentValidation automáticamente
        var failures = _validators.Select(v => v.Validate(request));
        if (failures.Any()) throw new DomainException(failures.First());
        return await next();
    }
}
```

### 3. Resiliencia con Polly

**Configuración en Program.cs:**
```csharp
builder.Services.AddHttpClient<IExternalPriceProvider, ExternalPriceProvider>(...)
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.ServiceUnavailable)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

### 4. Patrón Singleton (State Management)

**Propósito:** Mantener un estado único y compartido en memoria.

**Implementación (Mock API):**
En el simulador de precios, es crucial que los cambios en el valor de un activo persistan entre diferentes llamadas a la API. Para lograr esto, el almacén de activos se registra como Singleton.

```csharp
// Program.cs del PriceMock API
builder.Services.AddSingleton<IAssetStore, AssetStore>();
```

Esto garantiza que todas las peticiones interactúen con la misma instancia de `AssetStore`, permitiendo simulaciones de mercado dinámicas donde el precio fluctúa globalmente.

## 🎯 Validación Declarativa (FluentValidation)

```csharp
public class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Ticker).NotEmpty();
        // ... otras reglas
    }
}
```

### 2. Inyección de Dependencias

**Configuración en Program.cs:**
```csharp
// Repositorios
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IFinancialAssetRepository, FinancialAssetRepository>();

// Servicios
builder.Services.AddScoped<IOrderService, OrderService>();

// HttpClient con configuración
builder.Services.AddHttpClient<IExternalPriceProvider, ExternalPriceProvider>(client =>
{
    var baseUrl = builder.Configuration.GetValue<string>("PriceApi:BaseUrl") 
        ?? "http://localhost:5001/";
    client.BaseAddress = new Uri(baseUrl);
});

// DbContext
builder.Services.AddDbContext<OmsDbContext>(options =>
    options.UseSqlite(connectionString));
```

### 3. Manejo de Excepciones Global

**Middleware en Program.cs:**
```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?.Error;
        
        // Mapear exception a response
        var response = MapExceptionToResponse(exception);
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    });
});
```

## 📝 DTOs (Data Transfer Objects)

Los DTOs se definen en la capa Application para transferencia de datos entre capas:

```csharp
// Request
public class OrderCreateRequest
{
    public int AccountId { get; set; }
    public string Ticker { get; set; }
    public int Quantity { get; set; }
    public char Operation { get; set; } // 'C' (Compra) o 'V' (Venta)
}

// Response
public class OrderResponse
{
    public int Id { get; set; }
    public string Ticker { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## 🗄️ Entity Framework Core

### DbContext
```csharp
public class OmsDbContext : DbContext
{
    public OmsDbContext(DbContextOptions<OmsDbContext> options)
        : base(options) { }

    public DbSet<InvestmentOrder> Orders { get; set; }
    public DbSet<FinancialAsset> Assets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración de entidades
        modelBuilder.Entity<InvestmentOrder>()
            .HasKey(o => o.Id);
        
        modelBuilder.Entity<FinancialAsset>()
            .HasKey(a => a.Id);
    }
}
```

### Creación de Base de Datos
```csharp
public static void EnsureDatabaseCreated(this IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OmsDbContext>();
    context.Database.EnsureCreated();
}
```

## 🎯 Validación

### Validación en Controllers
```csharp
[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    try
    {
        var order = await _orderService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
    catch (DomainException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

## 🔄 Servicio de Órdenes

**Responsabilidades principales:**
```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IFinancialAssetRepository _assetRepository;
    private readonly IExternalPriceProvider _priceProvider;

    public async Task<OrderResponse> CreateOrderAsync(OrderCreateRequest request)
    {
        // 1. Validar activo existe
        var asset = await _assetRepository.GetByTickerAsync(request.Ticker);
        if (asset == null)
            throw new NotFoundException($"Activo {request.Ticker} no encontrado");

        // 2. Obtener precio actual
        var price = await _priceProvider.GetPriceAsync(request.Ticker);

        // 3. Crear orden con lógica de negocio
        var order = new InvestmentOrder
        {
            AccountId = request.AccountId,
            Ticker = request.Ticker,
            Quantity = request.Quantity,
            Price = price,
            Operation = request.Operation,
            // ... otros campos
        };

        // 4. Calcular comisiones y impuestos
        CalculateFeesAndTaxes(order);

        // 5. Persistir
        await _orderRepository.AddAsync(order);
        
        return MapToResponse(order);
    }
}
```

## 🌐 Cliente HTTP para Servicio Externo

```csharp
public class ExternalPriceProvider : IExternalPriceProvider
{
    private readonly HttpClient _httpClient;

    public async Task<decimal> GetPriceAsync(string ticker)
    {
        try
        {
            var response = await _httpClient.GetAsync($"prices/{ticker}");
            
            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException("Error obteniendo precio");

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<PriceResponse>(json);
            
            return data?.Price ?? throw new ExternalServiceException("Precio no disponible");
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException("Error de conexión con servicio de precios", ex);
        }
    }
}
```

## 🧪 Pruebas Unitarias

**Estructura de test:**
```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IFinancialAssetRepository> _assetRepositoryMock;
    private readonly Mock<IExternalPriceProvider> _priceProviderMock;
    private readonly OrderService _sut; // System Under Test

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _assetRepositoryMock = new Mock<IFinancialAssetRepository>();
        _priceProviderMock = new Mock<IExternalPriceProvider>();
        
        _sut = new OrderService(
            _orderRepositoryMock.Object,
            _assetRepositoryMock.Object,
            _priceProviderMock.Object);
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ReturnsOrderResponse()
    {
        // Arrange
        var request = new OrderCreateRequest { /* ... */ };
        // ... setup mocks

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Quantity, result.Quantity);
    }
}
```

## 📋 Flujo de Petición

```
1. HTTP Request → Controller
2. Controller valida y llama a Service
3. Service valida lógica de negocio
4. Service usa Repositories para acceso a datos
5. Repository usa DbContext con EF Core
6. DbContext genera SQL y ejecuta en SQLite
7. Response se mapea a DTO
8. Controller retorna JSON con HTTP Status
```

## 🔐 Validación de Dominio

Ejemplo en la entidad:

```csharp
public sealed class InvestmentOrder
{
    public void SetStatus(OrderStatus status)
    {
        if (Status == status)
            return; // Validar estado no repetido
        
        Status = status;
    }
}
```

## 📊 Configuración por Ambiente

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "PriceApi": {
    "BaseUrl": "http://localhost:5001/"
  }
}
```
