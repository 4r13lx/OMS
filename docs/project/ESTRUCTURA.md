# Estructura del Proyecto OMS

## 📁 Árbol de Directorios

```
OMS/
├── Oms.Apis/
│   ├── Oms.Api/                      # API Principal
│   │   ├── Controllers/              # Controladores REST
│   │   ├── Infrastructure/           # Infraestructura local (GlobalExceptionHandler, etc.)
│   │   ├── Properties/               # Configuración de launch
│   │   ├── Program.cs                # Configuración de la aplicación y Pipeline Polly
│   │   ├── Oms.Api.csproj           # Archivo del proyecto
│   │   ├── Oms.Api.http             # Solicitudes HTTP para prueba
│   │   └── appsettings.*.json       # Configuración por ambiente
│   │
│   └── Oms.PriceMock.Api/            # API Mock de Precios
│       ├── Program.cs                # Configuración y endpoints
│       ├── Oms.PriceMock.Api.csproj # Archivo del proyecto
│       ├── Oms.PriceMock.Api.http   # Solicitudes HTTP para prueba
│       └── appsettings.*.json       # Configuración por ambiente
│
├── Oms.Core/                         # Capa de Dominio (Pureza e Independencia)
│   ├── Entities/
│   │   ├── InvestmentOrder.cs        # Entidad de Orden con lógica de estado
│   │   └── FinancialAsset.cs         # Entidad de Activo
│   ├── Enums/
│   │   ├── OrderStatus.cs            # Estados de la orden
│   │   ├── OperationType.cs          # Tipos de operación
│   │   └── AssetType.cs              # Tipos de activo
│   ├── Exceptions/
│   │   ├── DomainException.cs        # Excepción base de negocio
│   │   ├── NotFoundException.cs      # Recurso no encontrado (404)
│   │   └── ExternalServiceException.cs # Error de servicio externo (503)
│   ├── Interfaces/
│   │   ├── IOrderRepository.cs       # Contrato del repositorio de órdenes
│   │   └── IFinancialAssetRepository.cs # Contrato del repositorio de activos
│   └── Oms.Core.csproj
│
├── Oms.Application/                  # Capa de Aplicación (Orquestación y Mensajería)
│   ├── Common/
│   │   └── Behaviors/
│   │       └── ValidationBehavior.cs # Pipeline de validación automática
│   ├── Dtos/
│   │   ├── OrderResponse.cs          # DTO de respuesta de orden
│   │   ├── OrderCreateRequest.cs     # DTO de creación de orden
│   │   ├── OrderUpdateRequest.cs     # DTO de actualización de orden
│   │   └── AccountSummaryResponse.cs # DTO de resumen de cuenta
│   ├── Orders/
│   │   ├── Commands/                 # Comandos (Escritura - CQRS)
│   │   └── Queries/                  # Consultas (Lectura - CQRS)
│   ├── Services/
│   │   ├── IOrderService.cs          # Interfaz legada de servicios
│   │   ├── OrderService.cs           # Implementación legada
│   │   └── IExternalPriceProvider.cs # Interfaz para precios de mercado
│   ├── Validators/
│   │   └── OrderCreateRequestValidator.cs # Reglas de validación declarativas
│   └── Oms.Application.csproj
│
├── Oms.Infrastructutre/              # Capa de Infraestructura Técnica
│   └── Oms.Infrastructure/
│       ├── Providers/
│       │   └── ExternalPriceMock/
│       │       ├── PriceStore.cs     # Almacén de activos en memoria
│       │       ├── RateLimitDecorator.cs # Patrón Decorator para límites
│       │       └── Service.cs        # Lógica de simulación de precios
│       └── Oms.Infrastructure.csproj
│
├── Oms.Data/                         # Capa de Datos (Persistencia)
│   ├── OmsDbContext.cs               # Contexto EF Core con Data Seeding
│   ├── Repositories/
│   │   ├── OrderRepository.cs        # Implementación concreta (SQL Lite)
│   │   └── FinancialAssetRepository.cs # Implementación concreta
│   └── Oms.Data.csproj
│
├── Oms.Test/                         # Capa de Calidad (Pruebas)
│   ├── OrderServiceTests.cs          # Tests con In-Memory DB y Stubs
│   ├── GlobalUsings.cs               # Importaciones globales
│   └── Oms.Test.csproj
│
├── docs/                             # Documentación del Sistema
│   └── project/                      # Guías detalladas
│       ├── README.md                 # Índice de documentación
│       ├── ARQUITECTURA.md           # Diseño de capas y patrones
│       ├── ESTRUCTURA.md             # Este archivo
│       ├── IMPLEMENTACION.md         # Guía técnica de patrones
│       ├── LOGICA.md                 # Reglas de negocio y cálculos
│       ├── TESTS.md                  # Estrategia de testing
│       └── DOCKER.md                 # Contenerización
│
├── Dockerfile.Api                    # Imagen para la API
├── Dockerfile.PriceMock              # Imagen para el Mock
├── docker-compose.yml                # Orquestación de servicios
├── OMS.sln                           # Solución de Visual Studio
├── .gitignore                        # Exclusiones de Git
└── README.md                         # Resumen del proyecto

```

## 📦 Proyectos y sus Responsabilidades

### Oms.Api
**Tipo:** Web API (.NET 8)  
**Responsabilidad:** Punto de entrada. Configura la resiliencia con **Polly**, el manejo global de errores y la inyección de dependencias. Delega la lógica pesada a **MediatR**.

### Oms.Infrastructure
**Tipo:** Librería de Clases (.NET 8)  
**Responsabilidad:** Implementaciones técnicas transversales. Contiene la lógica del simulador de precios, implementa el **Patrón Decorator** para el control de tráfico y gestiona los activos mock.

### Oms.Core
**Tipo:** Librería de Clases (.NET 8)  
**Responsabilidad:** Núcleo del sistema. Contiene el estado puro y las interfaces que definen el comportamiento de la aplicación sin dependencias de frameworks.

### Oms.Application
**Tipo:** Librería de Clases (.NET 8)  
**Responsabilidad:** Orquestación de procesos. Implementa **CQRS** (Comandos y Consultas), validación automática mediante **Pipeline Behaviors** y transformación de datos (DTOs).

### Oms.Data
**Tipo:** Librería de Clases (.NET 8)  
**Responsabilidad:** Persistencia. Implementa el patrón **Repository** sobre EF Core para desacoplar el motor de base de datos (SQLite) del resto del sistema.

### Oms.Test
**Tipo:** Proyecto de Tests (xUnit)  
**Responsabilidad:** Validación de la lógica crítica. Utiliza **In-Memory Databases** para velocidad y **Stubs** para aislar dependencias externas.

## 🔗 Dependencias entre Proyectos

```
Oms.Api
  ├── Oms.Application ──→ Oms.Core
  ├── Oms.Data ─────────→ Oms.Core
  └── Oms.Infrastructure ─→ Oms.Core

Oms.Test
  ├── Oms.Application
  └── Oms.Data
```
