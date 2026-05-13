# Guía de Docker y Containerización

## 🐳 Visión General

El proyecto OMS está completamente containerizado usando Docker y docker-compose para facilitar deployment y desarrollo.

### Componentes
- **Dockerfile.Api** - Imagen para API principal (Oms.Api)
- **Dockerfile.PriceMock** - Imagen para API de precios (Oms.PriceMock.Api)
- **docker-compose.yml** - Orquestación de servicios

## 📋 Requisitos Previos

```bash
# Verificar Docker instalado
docker --version
# Output: Docker version 24.0.0 (o superior)

# Verificar Docker Compose instalado
docker-compose --version
# Output: Docker Compose version 2.0.0 (o superior)
```

## 🏗️ Construcción de Imágenes

### Opción 1: Con docker-compose (RECOMENDADO)

```bash
# Construir y ejecutar todos los servicios
docker-compose up --build

# Solo construir (sin ejecutar)
docker-compose build

# Construir servicio específico
docker-compose build oms-api
docker-compose build price-mock-api
```

### Opción 2: Construir manualmente

```bash
# Construir imagen de API principal
docker build -f Dockerfile.Api -t oms-api:latest .

# Construir imagen de API de precios
docker build -f Dockerfile.PriceMock -t oms-price-mock:latest .

# Verificar imágenes creadas
docker images | grep oms
```

## 🚀 Ejecución

### Usar docker-compose (RECOMENDADO)

```bash
# Ejecutar en foreground (ver logs en tiempo real)
docker-compose up

# Ejecutar en background (detached)
docker-compose up -d

# Detener servicios
docker-compose down

# Detener y eliminar volúmenes
docker-compose down -v

# Ver logs
docker-compose logs -f

# Ver logs de servicio específico
docker-compose logs -f oms-api
docker-compose logs -f price-mock-api

# Reiniciar servicios
docker-compose restart

# Rebuild sin caché
docker-compose up --build --no-cache
```

### Ejecutar contenedores manualmente

```bash
# Ejecutar API de precios (primero)
docker run -d \
  --name price-mock-api \
  -p 5001:5001 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:5001 \
  oms-price-mock:latest

# Ejecutar API principal
docker run -d \
  --name oms-api \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:5000 \
  -e PriceApi__BaseUrl=http://localhost:5001/ \
  -e ConnectionStrings__OmsDb=Data Source=/app/data/oms.db \
  -v oms-data:/app/data \
  oms-api:latest

# Ver contenedores en ejecución
docker ps

# Ver logs
docker logs -f oms-api
```

## 🌐 Acceso a los Servicios

### Cuando docker-compose está ejecutándose

| Servicio | URL | Descripción |
|----------|-----|-------------|
| API Principal | http://localhost:5000 | API de gestión de órdenes |
| Swagger API | http://localhost:5000/swagger | Documentación interactiva |
| Price Mock API | http://localhost:5001 | API de precios mock |
| Swagger Prices | http://localhost:5001/swagger | Documentación de precios |

### Ejemplo de Solicitud

```bash
# Obtener precio de AAPL
curl http://localhost:5001/prices/AAPL

# Crear orden
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "accountId": 1,
    "ticker": "AAPL",
    "quantity": 100,
    "operation": "C"
  }'

# Ver órdenes de cuenta
curl http://localhost:5000/api/accounts/1/orders
```

## 📦 Estructura de Dockerfiles

### Dockerfile.Api

```dockerfile
# Build stage - Compila la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia proyectos
COPY ["OMS.sln", "."]
COPY ["Oms.Apis/Oms.Api/Oms.Api.csproj", "Oms.Apis/Oms.Api/"]
# ... otros proyectos

# Restaura dependencias y compila
RUN dotnet restore
RUN dotnet publish "Oms.Apis/Oms.Api/Oms.Api.csproj" -c Release -o /app/publish

# Runtime stage - Ejecuta la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copia binarios compilados
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Oms.Api.dll"]
```

**Ventajas de multi-stage:**
- ✅ Imagen final más pequeña (solo runtime, no SDK)
- ✅ Construcción aislada
- ✅ Mejor seguridad (sin código fuente en imagen)

## 🔧 Docker Compose Detallado

```yaml
version: '3.8'

services:
  # API de Precios Mock - Inicia primero
  price-mock-api:
    build:
      context: .                          # Raíz del proyecto
      dockerfile: Dockerfile.PriceMock    # Dockerfile a usar
    container_name: oms-price-mock-api    # Nombre del contenedor
    ports:
      - "5001:5001"                       # Port mapping
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5001
    networks:
      - oms-network                       # Red compartida
    restart: unless-stopped               # Política de reinicio

  # API Principal - Depende de price-mock-api
  oms-api:
    build:
      context: .
      dockerfile: Dockerfile.Api
    container_name: oms-api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      - PriceApi__BaseUrl=http://price-mock-api:5001/
      - ConnectionStrings__OmsDb=Data Source=/app/data/oms.db
    volumes:
      - oms-data:/app/data                # Volumen para persistencia
    networks:
      - oms-network
    depends_on:
      - price-mock-api                    # Espera a que price-mock esté listo
    restart: unless-stopped

networks:
  oms-network:
    driver: bridge

volumes:
  oms-data:                               # Volumen nombrado para datos
```

## 💾 Volúmenes y Persistencia

### Verificar volúmenes

```bash
# Listar volúmenes
docker volume ls

# Inspeccionar volumen
docker volume inspect oms_oms-data

# Ver contenido del volumen (desde contenedor)
docker run -v oms_oms-data:/data alpine ls /data

# Limpiar volúmenes no usados
docker volume prune
```

### Backup de base de datos

```bash
# Copiar base de datos del volumen
docker run --rm -v oms_oms-data:/data -v $(pwd):/backup \
  alpine cp /data/oms.db /backup/oms.db

# Restaurar desde backup
docker run --rm -v oms_oms-data:/data -v $(pwd):/backup \
  alpine cp /backup/oms.db /data/oms.db
```

## 🔍 Debugging y Troubleshooting

### Ver logs

```bash
# Todos los servicios
docker-compose logs

# Filtrar por servicio
docker-compose logs oms-api

# Últimas 100 líneas
docker-compose logs --tail=100

# Seguir en tiempo real
docker-compose logs -f

# Timestamps incluidos
docker-compose logs --timestamps
```

### Acceder al contenedor

```bash
# Shell interactivo en contenedor en ejecución
docker-compose exec oms-api bash

# Ejecutar comando en contenedor
docker-compose exec oms-api ls /app

# Verificar procesos
docker-compose exec oms-api ps aux

# Ver variables de entorno
docker-compose exec oms-api env
```

### Problemas Comunes

**Error: "Port already in use"**
```bash
# Encontrar qué usa el puerto
lsof -i :5000

# Liberar puerto (si es necesario)
kill -9 <PID>

# O usar puerto diferente en docker-compose
# Cambiar "5000:5000" a "5005:5000"
```

**Error: "Cannot connect to price-mock-api"**
```bash
# Verificar que el servicio está corriendo
docker-compose ps

# Verificar conectividad entre contenedores
docker-compose exec oms-api curl http://price-mock-api:5001/prices/AAPL
```

**Base de datos corrupta**
```bash
# Eliminar volumen y reiniciar (CUIDADO: pierde datos)
docker-compose down -v
docker-compose up
```

## 📊 Estadísticas y Monitoreo

```bash
# Ver uso de recursos en tiempo real
docker stats

# Inspeccionar contenedor
docker inspect oms-api

# Ver historial de cambios
docker history oms-api:latest
```

## 🏢 Deployment a Producción

### En servidor Linux

```bash
# Clonar repositorio
git clone <repo-url>
cd OMS

# Ejecutar con variables de entorno personalizadas
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Ver estado
docker-compose ps

# Ver logs
docker-compose logs -f
```

### Con Docker Swarm (múltiples servidores)

```bash
# Inicializar swarm
docker swarm init

# Desplegar stack
docker stack deploy -c docker-compose.yml oms

# Verificar servicios
docker service ls
```

### Con Kubernetes (clusters)

```bash
# Aplicar manifiestos (requiere archivos YAML)
kubectl apply -f kubernetes/

# Verificar deployments
kubectl get deployments
kubectl get pods
```

## 🧹 Limpieza

```bash
# Detener y eliminar contenedores, redes (mantiene imágenes y volúmenes)
docker-compose down

# Eliminar todo incluyendo volúmenes
docker-compose down -v

# Eliminar imágenes también
docker-compose down -v --rmi all

# Limpiar recursos no usados
docker system prune

# Limpiar todo (CUIDADO)
docker system prune -a
```

## 📚 Recursos Adicionales

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
