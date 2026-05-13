# Arquitectura del Proyecto OMS

## 🏗️ Visión General

El proyecto OMS (Order Management System) sigue una arquitectura en capas con separación clara de responsabilidades. Está diseñado siguiendo principios de **Domain-Driven Design (DDD)** y **Clean Architecture**.

### Capas Principales y Relaciones

```
┌───────────────────────────────────────────────┐
│           API Layer (Oms.Api)                 │
│   (Controllers, Middlewares, Resiliency)      │
└────────┬──────────────────────┬───────────────┘
         │                      │
┌────────▼──────────────────────▼───────────────┐
│     Application Layer (Oms.Application)       │
│  (Handlers, Queries, Commands, DTOs, Behaviors)│
└────────┬──────────────────────┬───────────────┘
         │                      │
┌────────▼────────────┐  ┌──────▼───────────────┐
│ Core Layer (Oms.Core)│  │ Infrastructure Layer │
│ (Domain, Entities,  │  │ (Oms.Infrastructure) │
│  Interfaces, Enums) │  │ (External Mock,      │
└────────▲────────────┘  │  Decorators, Sim)    │
         │               └──────┬───────────────┘
┌────────┴────────────┐         │
│  Data Layer         │◄────────┘
│  (Oms.Data)         │
│ (EF Core, Repo)     │
└─────────────────────┘
```

## 📚 Descripción de Capas

### 1. **Core Layer (Oms.Core)**
Contiene la lógica de dominio pura, independiente de cualquier framework.

**Responsabilidades:**
- Definir **entidades del dominio** (InvestmentOrder, FinancialAsset) con lógica de estado encapsulada.
- Definir **enumeraciones** (OrderStatus, OperationType, AssetType).
- Definir **excepciones de dominio** (jerarquía para manejo semántico de errores).
- Definir **interfaces de repositorio** (Contratos de persistencia).

**Principio:** No tiene dependencias externas excepto .NET Framework estándar.

### 2. **Application Layer (Oms.Application)**
Orquesta las operaciones entre el Core y la Infraestructura. Implementa el patrón **Mediator** para desacoplar casos de uso.

**Responsabilidades:**
- Implementar **Handlers de MediatR** para Comandos (escritura) y Consultas (lectura).
- Implementar **Pipeline Behaviors** (ej. Validación automática con FluentValidation).
- Definir **DTOs** para transferencia de datos (OrderCreateRequest, OrderResponse, etc.).
- Implementar la **lógica de validación** declarativa con **FluentValidation**.

**Patrón:** Mediator (CQRS Lite) + Validation Pipeline.

### 3. **Infrastructure Layer (Oms.Infrastructure)**
Capa dedicada a las implementaciones técnicas y servicios externos que no pertenecen a la persistencia de datos.

**Responsabilidades:**
- Implementar **simuladores de servicios externos** (PriceMock).
- Implementar patrones de comportamiento técnico como el **Decorator** para límites de tasa (Rate Limiting).
- Gestionar **proveedores de datos técnicos** y lógica de simulación de mercado.

**Principio:** Depende del Core para las interfaces, pero contiene lógica técnica específica.

### 4. **Data Layer (Oms.Data)**
Maneja la persistencia de datos y acceso a base de datos.

**Responsabilidades:**
- Implementar **DbContext** (Entity Framework Core con SQLite).
- Implementar **Repositories** concretos (OrderRepository, FinancialAssetRepository).
- Configurar **mappings** y **Data Seeding** (datos iniciales de activos).

**Tecnología:** Entity Framework Core con SQLite.

### 5. **API Layer (Oms.Api)**
Presenta la interfaz HTTP/REST del sistema. Capa delgada (Thin Controllers).

**Responsabilidades:**
- Definir **Controllers** que delegan la lógica a MediatR.
- Configurar el **Pipeline de Middleware** de ASP.NET Core.
- Implementar **Global Exception Handling** con soporte para **Problem Details (RFC 7807)**.
- Configurar **HttpClient con Resiliencia (Polly)**.

## 🎯 Patrones de Diseño

### Mediator Pattern (MediatR)
- Desacopla los controladores de la lógica de negocio.
- Cada caso de uso (ej. Crear Orden) es un mensaje independiente (Command/Query).

### Pipeline Behaviors
- Permite inyectar lógica transversal (ej. validación, logging) de forma transparente antes de ejecutar un Handler.

### Repository Pattern
- Abstrae el acceso a datos.
- Proporciona una interfaz similar a una colección de objetos en memoria.

### Decorator Pattern
- Utilizado en la infraestructura para añadir funcionalidades (ej. Rate Limiting) a servicios existentes sin modificar su código original.

### Singleton Pattern
- Utilizado para gestionar el estado global en memoria dentro de los simuladores (ej. `AssetStore`).
- Asegura que exista una única instancia de los datos durante todo el ciclo de vida de la aplicación, permitiendo que las variaciones de precios persistan entre diferentes peticiones HTTP.

### Resiliency Patterns (Polly)
- **Retry with Exponential Backoff:** Reintenta llamadas a servicios externos fallidos con tiempos de espera crecientes.
- Manejo automático de errores transitorios y códigos específicos como HTTP 503.

## 🔄 Flujo de Datos

```
HTTP Request → Controller → Mediator (Pipeline Behaviors) → Handler → Repository → DbContext → Database
                                                                 ↓
                                                       External Service (Polly Policy)
```

## 🛡️ Manejo de Errores

Jerarquía de excepciones:

```
Exception
├── DomainException (lógica de negocio)
├── NotFoundException (recurso no encontrado)
├── ExternalServiceException (errores de servicios externos)
└── Middleware Global Exception Handler
```

## 🔐 Seguridad

- **Validación de entrada**: En DTOs y Controllers
- **Manejo de excepciones**: Global Exception Handler middleware
- **SQLite**: Base de datos embebida, sin exposición de red
- **CORS**: Configurable según necesidad

## 📊 Bases de Datos

**Motor:** SQLite
- Archivo: `oms.db`
- Tablas principales:
  - `InvestmentOrders` - Órdenes de inversión
  - `FinancialAssets` - Activos financieros

## 🚀 Escalabilidad

Para producción:
- Migrar a SQL Server / PostgreSQL
- Implementar caché (Redis)
- Agregar eventos de dominio
- Implementar CQRS si es necesario
- Agregar circuit breaker para servicios externos
