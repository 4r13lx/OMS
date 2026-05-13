# Guía de Testing

## 🧪 Estrategia de Testing

El proyecto utiliza **xUnit** para tests unitarios con mock de dependencias.

### Pirámide de Testing
```
        ┌─────────────┐
        │ E2E Tests   │ (Bajo volumen, alto costo)
        ├─────────────┤
        │  Integration│ (Medio volumen)
        ├─────────────┤
        │ Unit Tests  │ (Alto volumen, bajo costo)
        └─────────────┘
```

Actualmente enfocados en **Unit Tests** del `OrderService`.

## 📁 Estructura de Tests

```
Oms.Test/
├── OrderServiceTests.cs
├── GlobalUsings.cs
└── Oms.Test.csproj
```

## 🏃 Ejecutar Tests

### Desde Terminal
```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con salida detallada
dotnet test --verbosity detailed

# Ejecutar tests de un proyecto específico
dotnet test Oms.Test/

# Ejecutar un test específico
dotnet test --filter "MethodName=CreateOrder_WithValidRequest_ReturnsOrderResponse"

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true

# Ejecutar con filtro de categoría
dotnet test --filter "Category=Integration"
```

### Desde VS Code
```
1. Click en "Run Test" arriba del método de test
2. O usar Command Palette: "Test: Run Tests"
```

## 🧩 Estructura de un Test

El proyecto utiliza un enfoque de **Tests Unitarios de Alta Velocidad** mediante el uso de bases de datos en memoria y dobles de prueba (Stubs).

### Ejemplo: OrderServiceTests

```csharp
public class OrderServiceTests
{
    // Aislamiento: El validador se instancia directamente para pruebas unitarias.
    private readonly IValidator<OrderCreateRequest> _validator = new OrderCreateRequestValidator();

    [Fact]
    public async Task CreateAccionOrder_UsesExternalPriceAndAppliesFees()
    {
        // ARRANGE: Usamos una base de datos en memoria (SQLite In-Memory o EF In-Memory)
        // Esto garantiza que cada test tenga un entorno limpio y sea extremadamente rápido.
        using var context = CreateContext("test-db");
        await SeedAssets(context);

        var orderRepository = new OrderRepository(context);
        var assetRepository = new FinancialAssetRepository(context);
        
        // Uso de STUB: Simulamos el servicio externo de precios para evitar llamadas de red.
        var externalPriceProvider = new StubPriceProvider(200m);
        
        var orderService = new OrderService(orderRepository, assetRepository, externalPriceProvider, _validator);

        var request = new OrderCreateRequest
        {
            AccountId = 2,
            Ticker = "AAPL",
            AssetName = "Apple",
            Quantity = 10,
            Operation = 'V'
        };

        // ACT
        var response = await orderService.CreateOrderAsync(request);

        // ASSERT: Verificamos los cálculos financieros esperados para una Acción
        Assert.Equal(2000m, response.TotalAmount);
        Assert.Equal(12m, response.CommissionAmount); // 0.6% de 2000
        Assert.Equal(2.52m, response.TaxAmount);       // 21% de 12
    }
}
```

## ✅ Patrones de Testing

### 1. In-Memory Databases
- Se utiliza `UseInMemoryDatabase` para simular la persistencia sin la sobrecarga de un motor de base de datos real.
- Ventaja: Aislamiento total entre tests y velocidad de ejecución (< 10ms por test).

### 2. Doubles de Prueba (Stubs)
- `StubPriceProvider`: Una implementación simplificada de `IExternalPriceProvider` que devuelve un precio fijo.
- Propósito: Eliminar la dependencia de servicios externos que pueden fallar o tener latencia, permitiendo probar únicamente la lógica de negocio de la orden.

### 2. Testing con Excepciones

```csharp
[Fact]
public async Task CreateOrder_WithNegativeQuantity_ThrowsDomainException()
{
    // ARRANGE
    var request = new OrderCreateRequest 
    { 
        Quantity = -10 
    };

    // ACT & ASSERT
    var exception = await Assert.ThrowsAsync<DomainException>(
        () => _sut.CreateOrderAsync(request));
    Assert.Contains("Cantidad", exception.Message);
}
```

### 3. Testing de Métodos Síncronos

```csharp
[Theory]
[InlineData(100, 'C')]
[InlineData(50, 'V')]
public void SetStatus_WithValidStatus_ChangesStatus(int quantity, char operation)
{
    // ARRANGE
    var order = new InvestmentOrder { Quantity = quantity, Operation = operation };

    // ACT
    order.SetStatus(OrderStatus.Ejecutada);

    // ASSERT
    Assert.Equal(OrderStatus.Ejecutada, order.Status);
}
```

## 🎯 Casos de Test Recomendados

### OrderService

#### CreateOrder
- ✅ `WithValidRequest_ReturnsOrderResponse` - Caso feliz
- ✅ `WithNonExistentTicker_ThrowsNotFoundException` - Activo no existe
- ✅ `WithNegativeQuantity_ThrowsDomainException` - Validación
- ✅ `WithPriceServiceFailure_ThrowsExternalServiceException` - Error externo
- ✅ `CalculatesCommissionAndTaxesCorrectly_WithValidData` - Cálculos

#### GetOrderByIdAsync
- ✅ `WithValidId_ReturnsOrder` - Orden existe
- ✅ `WithInvalidId_ThrowsNotFoundException` - Orden no existe
- ✅ `WithUnauthorizedAccount_ThrowsDomainException` - Acceso denegado

#### GetAccountSummaryAsync
- ✅ `WithMultipleOrders_ReturnsSummary` - Resumen correcto
- ✅ `WithNoOrders_ReturnsZeroSummary` - Sin órdenes

#### UpdateOrderStatusAsync
- ✅ `WithValidTransition_UpdatesStatus` - Transición válida
- ✅ `WithInvalidTransition_ThrowsDomainException` - Transición inválida
- ✅ `WithSameStatus_DoesNotUpdate` - Estado igual

## 🔧 Mocking con Moq

### Setup Básico
```csharp
// Retornar valor específico
_repository.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(order);

// Lanzar excepción
_repository.Setup(r => r.GetByIdAsync(-1))
    .ThrowsAsync(new NotFoundException());

// Callback
_repository.Setup(r => r.AddAsync(It.IsAny<Order>()))
    .Callback<Order>(o => o.Id = 999)
    .Returns(Task.CompletedTask);
```

### Verificación de Llamadas
```csharp
// Verificar que se llamó exactamente 1 vez
_repository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);

// Verificar que NUNCA se llamó
_repository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);

// Verificar con parámetros específicos
_repository.Verify(r => r.GetByIdAsync(1), Times.AtLeastOnce);
```

## 📊 Cobertura de Código

### Generar Reporte de Cobertura
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Objetivos de Cobertura
- **Métodos públicos:** 90%+ cobertura
- **Lógica crítica:** 100% cobertura
- **Getters/Setters simples:** 70%+ cobertura

## 🚀 CI/CD Testing

En pipeline CI/CD:
```bash
# Ejecutar tests y fallar si alguno falla
dotnet test --no-build --logger "trx;LogFileName=test-results.trx"

# Con reporte de cobertura
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

## 💡 Best Practices

1. **Nombres descriptivos:** `CreateOrder_WithValidRequest_ReturnsOrderResponse`
2. **Un assert principal por test:** No mezclar múltiples verificaciones
3. **Datos de prueba realistas:** Usar valores cercanos a producción
4. **Mocks mínimos:** Solo lo necesario
5. **Independencia:** Tests sin dependencias entre ellos
6. **Rapidez:** Tests unitarios < 1 segundo cada uno
7. **Documentación:** Comentarios en tests complejos

## 📝 Fixture de Datos

```csharp
public static class TestDataFactory
{
    public static InvestmentOrder CreateTestOrder(
        int id = 1, 
        string ticker = "AAPL", 
        int quantity = 100)
    {
        return new InvestmentOrder
        {
            Id = id,
            Ticker = ticker,
            Quantity = quantity,
            Price = 177.97m,
            Operation = 'C',
            Status = OrderStatus.EnProceso,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// Uso en tests
var order = TestDataFactory.CreateTestOrder();
```

## 🔍 Debugging de Tests

### En VS Code
```
1. Click en "Debug" arriba del test
2. O usar F5 con breakpoint
3. Inspeccionar variables en Debug Console
```

### Desde Terminal
```bash
# Con información detallada
dotnet test --verbosity detailed

# Con salida de diagnósticos
dotnet test --diag test.log
```

## 🎓 Recursos

- [xUnit.net Documentation](https://xunit.net/)
- [Moq GitHub](https://github.com/moq/moq4)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
