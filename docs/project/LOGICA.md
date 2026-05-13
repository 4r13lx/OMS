# Lógica de Negocio

## 📊 Conceptos Principales

### Orden de Inversión (InvestmentOrder)
Representa una solicitud para comprar o vender un activo financiero.

**Atributos:**
- `Id`: Identificador único
- `AccountId`: Cuenta del inversor
- `Ticker`: Símbolo del activo (ej: AAPL, GOOGL)
- `AssetName`: Nombre del activo
- `AssetType`: Tipo de activo (Acción, Bonos, etc.)
- `Quantity`: Cantidad de unidades
- `Price`: Precio unitario en el momento de la orden
- `Operation`: Tipo de operación ('C' = Compra, 'V' = Venta)
- `Status`: Estado actual de la orden
- `TotalAmount`: Importe total de la operación
- `CommissionAmount`: Comisión cobrada
- `TaxAmount`: Impuestos aplicables
- `CreatedAt`: Fecha de creación

### Activo Financiero (FinancialAsset)
Representa un instrumento financiero disponible para negociación.

**Atributos:**
- `Id`: Identificador único
- `Ticker`: Símbolo único del activo
- `Name`: Nombre del activo
- `AssetType`: Tipo de activo
- `Sector`: Sector industrial (si aplica)

### Estado de la Orden (OrderStatus)
Estados posibles de una orden:

| Estado | Valor | Descripción |
|--------|-------|-------------|
| **EnProceso** | 0 | Orden recién creada, en validación |
| **Ejecutada** | 1 | Orden completada exitosamente |
| **Cancelada** | 3 | Orden cancelada |

## 💼 Procesos de Negocio

### 1. Crear Orden

**Flujo:**
```
1. Cliente envía solicitud de creación (OrderCreateRequest)
   ↓
2. Validación automática (FluentValidation Pipeline)
   ↓
3. Validar que el activo existe en el catálogo
   ↓
4. Resolución de precios según Tipo de Activo:
   - Acciones: Precio de mercado externo (Provider + Polly)
   - Bonos/FCI: Precio manual proporcionado en el request
   ↓
5. Crear orden en estado inicial "EnProceso"
   ↓
6. Calcular Comisiones e Impuestos según Tipo de Activo:
   - Acciones: Comisión 0.6%, IVA sobre comisión 21%
   - Bonos: Comisión 0.2%, IVA sobre comisión 21%
   - FCI: Sin comisiones ni impuestos
   ↓
7. Guardar en base de datos mediante Repositorio
   ↓
8. Retornar DTO de respuesta (OrderResponse)
```

**Validaciones:**
- El activo debe existir en el catálogo autorizado.
- Cantidad > 0.
- Operación 'C' (Compra) o 'V' (Venta).
- Para Bonos y FCI, el precio debe ser proporcionado y ser positivo.

**Ejemplo (Acción):**
```
Request: Comprar 10 acciones de AAPL (Precio Mercado: $200)
Importe Base: 10 × $200 = $2,000.00
Comisión (0.6%): $2,000.00 × 0.006 = $12.00
Impuestos (21% s/com): $12.00 × 0.21 = $2.52
Monto Total: $2,000.00 + $12.00 + $2.52 = $2,014.52
```

**Ejemplo (FCI):**
```
Request: Comprar 1000 cuotas de Delta.Pesos (Precio: $0.0181)
Importe Base: 1000 × $0.0181 = $18.10
Comisión: $0.00
Impuestos: $0.00
Monto Total: $18.10
```

## 🎯 Reglas de Negocio

### Comisiones por Tipo de Activo
| Tipo | Comisión |
|------|----------|
| **Acción** | 0.6% del importe total |
| **Bono** | 0.2% del importe total |
| **FCI** | 0.0% (Exento) |

### Impuestos
| Tipo | Impuesto | Aplicación |
|------|----------|------------|
| **Acción** | 21% (IVA) | Sobre el valor de la comisión |
| **Bono** | 21% (IVA) | Sobre el valor de la comisión |
| **FCI** | 0% | No aplica |

### Validaciones Específicas
1. **Cantidad mínima:** Mínimo 1 unidad
2. **Precisión de precio:** Máximo 4 decimales
3. **Ticker válido:** Debe estar en el sistema
4. **Operación válida:** 'C' (Compra) o 'V' (Venta)

## 🔗 Relaciones

```
Account (implícito mediante AccountId)
    ↓
    └─→ InvestmentOrder
            ├─→ FinancialAsset (por Ticker)
            └─→ Comisión y Impuestos calculados
```

## 🚨 Manejo de Errores

### Excepciones de Dominio

**NotFoundException:**
- Se lanza cuando un activo no existe
- Código HTTP: 404
- Ejemplo: `Activo XXXXX no encontrado`

**DomainException:**
- Se lanza en validaciones de lógica de negocio
- Código HTTP: 400
- Ejemplo: `Cantidad debe ser mayor a cero`

**ExternalServiceException:**
- Se lanza cuando el servicio de precios falla
- Código HTTP: 503
- Ejemplo: `Error de conexión con servicio de precios`

## 📈 Datos de Ejemplo

### Activos Disponibles
```
AAPL    - Apple Inc.           - Acción - $177.97
GOOGL   - Alphabet Inc.        - Acción - $138.21
MSFT    - Microsoft Corporation - Acción - $329.04
KO      - The Coca-Cola Company - Acción - $58.30
WMT     - Walmart Inc.         - Acción - $163.42
AL30    - Bono Argentino 2030  - Bono   - $307.40
GD30    - Bono Global 2030     - Bono   - $336.10
```

### Flujo de Ejemplo Completo

**Escenario:** Juan compra 50 acciones de AAPL

```
1. Request:
   POST /api/orders
   {
     "accountId": 1,
     "ticker": "AAPL",
     "quantity": 50,
     "operation": "C"
   }

2. Sistema obtiene precio: $177.97

3. Cálculos:
   - Importe base: 50 × $177.97 = $8,898.50
   - Comisión: $8,898.50 × 0.005 = $44.4925
   - Impuestos: $8,898.50 × 0.003 = $26.6955

4. Orden creada:
   {
     "id": 101,
     "accountId": 1,
     "ticker": "AAPL",
     "assetName": "Apple Inc.",
     "quantity": 50,
     "price": 177.97,
     "operation": "C",
     "totalAmount": 8898.50,
     "commissionAmount": 44.4925,
     "taxAmount": 26.6955,
     "status": "EnProceso",
     "createdAt": "2026-05-07T10:30:00Z"
   }

5. Se guarda en base de datos
```

## 🔄 Ciclo de Vida de una Orden

```
INICIO
  ↓
[EnProceso] ← Validar
  ↓
  ├─→ [Ejecutada] ← Completada exitosamente
  │
  └─→ [Cancelada] ← Cancelada por error/usuario
  
  [Ejecutada] y [Cancelada] son estados terminales
```

## 📋 Consideraciones de Escalabilidad

### Mejoras Futuras
- Implementar eventos de dominio (EventSourcing)
- Agregar historial de cambios
- Implementar auditoría
- Agregar notificaciones a clientes
- Implementar confirmación de órdenes
- Agregar límites de riesgo
- Implementar T+2 o liquidación
