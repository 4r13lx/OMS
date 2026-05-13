[![.NET](https://github.com/4r13lx/OMS/actions/workflows/dotnet.yml/badge.svg)](https://github.com/4r13lx/OMS/actions/workflows/dotnet.yml)

# OMS Investment Orders API

Solución para el desafío Backend Engineer Senior de PPI: gestión de órdenes de inversión con arquitectura en capas, integración con un mock de precios externos y reglas de negocio por tipo de activo.

## Proyectos

- `Oms.Api`: API principal REST para gestionar órdenes y consultar resúmenes de cuenta.
- `Oms.Core`: dominio, enumeraciones, excepciones e interfaces de repositorio.
- `Oms.Application`: casos de uso, validaciones, servicios de aplicación y DTOs.
- `Oms.Data`: persistencia con EF Core, contexto, repositorios y datos semilla.
- `Oms.PriceMock.Api`: API mock de precios externos con variación controlada y errores aleatorios.
- `Oms.Test`: pruebas unitarias para la lógica crítica de órdenes.

## Requisitos cumplidos

- CRUD completo para órdenes de inversión.
- Cálculo automático de `MontoTotal`, comisión e impuestos según tipo de activo.
- Reglas de negocio: estado inicial `EnProceso` y solo se puede modificar el estado después de crear la orden.
- Persistencia desacoplada con EF Core y `Sqlite`.
- Documentación de endpoints con Swagger en ambas APIs.
- API mock independiente para precios externos.
- Endpoint de resumen de cuenta.
- Manejo de errores con códigos HTTP adecuados.

## Cómo ejecutar

### 1. Restaurar y compilar

```bash
cd /home/sole/.source/repos/OMS
dotnet restore
dotnet build OMS.sln
```

### 2. Ejecutar la API principal

```bash
dotnet run --project Oms.Api/Oms.Api.csproj
```

### 3. Ejecutar el mock de precios

```bash
dotnet run --project Oms.PriceMock.Api/Oms.PriceMock.Api.csproj
```

### 4. Probar la API

- API principal: `https://localhost:5000/swagger` (o el puerto asignado)
- Mock de precios: `https://localhost:5001/swagger` (o el puerto asignado)

> Ajustar `PriceApi:BaseUrl` en `appsettings.json` de `Oms.Api` si el mock se ejecuta en un puerto distinto.

### 5. Ejecutar pruebas

```bash
dotnet test OMS.sln --no-restore
```

## Diseño arquitectónico

### Capa `Oms.Core`

Contiene el modelo de dominio, las entidades `InvestmentOrder` y `FinancialAsset`, las enumeraciones de estado y tipo de activo, y las interfaces de repositorio. Esta capa define las reglas de negocio y evita dependencias de infraestructura.

### Capa `Oms.Application`

Orquesta casos de uso de negocio:
- Validación de requests.
- Resolución de precios por tipo de activo.
- Cálculo de `TotalAmount`, comisiones e impuestos.
- Restricción de cambios de estado.
- Generación del resumen de cuenta.

### Capa `Oms.Data`

Implementa persistencia con EF Core y Sqlite:
- `OmsDbContext` con entidades `Orders` y `FinancialAssets`.
- Repositorios concretos.
- Datos semilla para los activos disponibles.

### Capa `Oms.Api`

Exposición de endpoints REST y manejo global de errores. Incluye:
- `OrdersController` para CRUD de órdenes.
- `AccountsController` para resumen de cuenta.
- `ExternalPriceProvider` para consumir el mock de precios.

### Mock de precios

`Oms.PriceMock.Api` expone `GET /prices/{ticker}` y simula:
- Variación controlada de precios entre llamadas.
- Fallos aleatorios con HTTP 503.
- Tickers válidos basados en la lista de activos autorizados.

## Consideraciones de diseño

- Se aplicó separación de responsabilidades para mantener el dominio independiente de la infraestructura.
- La persistencia se abstrajo en repositorios para facilitar el cambio de base de datos sin impacto en la lógica de negocio.
- Las validaciones de input se colocaron en la capa de aplicación, con excepciones de dominio para respuestas HTTP claras.
- Se emplea Swagger para documentación y facilidad de prueba.

## Uso de Inteligencia Artificial

- Sí, se utilizó una herramienta de IA para acelerar el diseño de la arquitectura en capas, generar la estructura inicial de proyectos y validar la lógica de negocio.
- Tareas específicas: análisis de requisitos, definición de proyectos, redacción de archivos de código, identificación de dependencias y ajuste de diseño.
- Prompt representativo:
  - "Diseña una arquitectura en capas para una API de órdenes de inversión con reglas diferentes según tipo de activo, persistencia EF Core y una API mock de precios externos."
- Revisión manual:
  - Se revisaron todos los cambios generados por IA para asegurar que las reglas de negocio sean correctas.
  - Se validaron las pruebas unitarias y la compilación completa de la solución.

## Notas

- El sistema está preparado para ser ampliado con Docker y pruebas de integración adicionales.
- El mock de precios puede ejecutarse en otro puerto y la API principal puede apuntar a él mediante configuración.
