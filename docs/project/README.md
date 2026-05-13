# Documentación del Proyecto OMS (Order Management System)

Bienvenido a la documentación del **Order Management System (OMS)**, un sistema de gestión de órdenes de inversión con API REST construido en .NET 8.

## 📋 Contenidos de Documentación

Esta carpeta contiene documentación detallada sobre distintos aspectos del proyecto:

- **[ARQUITECTURA.md](ARQUITECTURA.md)** - Descripción de la arquitectura general del sistema, patrones y principios de diseño
- **[ESTRUCTURA.md](ESTRUCTURA.md)** - Estructura de directorios y descripción de cada proyecto
- **[IMPLEMENTACION.md](IMPLEMENTACION.md)** - Detalles técnicos de implementación, patrones y buenas prácticas
- **[LOGICA.md](LOGICA.md)** - Lógica de negocio específica del dominio
- **[TESTS.md](TESTS.md)** - Guía de tests unitarios y cómo ejecutarlos
- **[DOCKER.md](DOCKER.md)** - Guía para compilar y ejecutar el proyecto con Docker

## 🚀 Inicio Rápido

### Requisitos
- .NET 8 SDK
- Docker y Docker Compose (opcional, para containerización)
- SQLite (incluido en las dependencias)

### Compilar el Proyecto
```bash
dotnet build
```

### Ejecutar Localmente
```bash
dotnet run --project Oms.Apis/Oms.Api
dotnet run --project Oms.Apis/Oms.PriceMock.Api
```

### Ejecutar con Docker
```bash
docker-compose up --build
```

## 📦 Descripción Breve

OMS es un sistema de gestión de órdenes de inversión que proporciona:

- **APIs REST** para crear, actualizar y consultar órdenes de inversión
- **Gestión de cuentas y activos financieros**
- **Servicio de precios externo** (mock) para obtener precios en tiempo real
- **Base de datos SQLite** para persistencia
- **Validación de lógica de negocio** en la capa de dominio
- **Tests unitarios** para garantizar calidad

## 🔗 Enlaces Importantes

- Repositorio: [OMS GitHub](#)
- API Principal: http://localhost:5000
- API de Precios Mock: http://localhost:5001
- Swagger UI (API): http://localhost:5000/swagger
- Swagger UI (Prices): http://localhost:5001/swagger
