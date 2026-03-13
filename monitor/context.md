# Monitor de Balanzas

Sistema de monitoreo en tiempo real para balanzas industriales conectadas en red.

## Descripción

Este proyecto permite registrar y monitorear el estado de conexión de múltiples balanzas industriales. El sistema realiza polling periódico a cada balanza registrada y actualiza su estado (conectado/desconectado) en tiempo real.

## Arquitectura

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │────▶│    Backend      │────▶│    MongoDB      │
│   React + Nginx │     │   .NET Core 8   │     │    (datos)      │
│   Puerto: 3000  │     │   Puerto: 5000  │     │  Puerto: 27017  │
└─────────────────┘     └────────┬────────┘     └─────────────────┘
                                 │
                                 ▼
                        ┌─────────────────┐
                        │    Balanzas     │
                        │  (red externa)  │
                        │ http://IP/balanza│
                        └─────────────────┘
```

## Tecnologías

| Componente | Tecnología | Versión |
|------------|------------|---------|
| Frontend | React | 18.x |
| Backend | .NET Core | 8.0 |
| Base de datos | MongoDB | 7.x |
| Contenedores | Docker Compose | - |
| Servidor web | Nginx | Alpine |

## Estructura del Proyecto

```
monitor/
├── docker-compose.yml          # Orquestación de contenedores
├── .gitignore                  # Archivos ignorados por git
├── context.md                  # Esta documentación
│
├── backend/
│   ├── .dockerignore
│   ├── Dockerfile              # Imagen Docker del backend
│   ├── BalanzasMonitor.csproj  # Proyecto .NET
│   ├── Program.cs              # Punto de entrada y configuración
│   ├── appsettings.json        # Configuración de logging
│   │
│   ├── Models/
│   │   └── Balanza.cs          # Modelo de datos
│   │
│   ├── Services/
│   │   └── MonitorService.cs   # Servicio de monitoreo (background)
│   │
│   └── Controllers/
│       └── BalanzasController.cs # API REST
│
└── frontend/
    ├── .dockerignore
    ├── Dockerfile              # Imagen Docker del frontend
    ├── package.json            # Dependencias npm
    ├── nginx.conf              # Configuración de Nginx
    │
    ├── public/
    │   └── index.html          # HTML base
    │
    └── src/
        ├── index.js            # Punto de entrada React
        ├── App.js              # Componente principal
        │
        ├── services/
        │   └── api.js          # Cliente HTTP (axios)
        │
        └── components/
            ├── BalanzaCard.js       # Card de cada balanza
            └── AddBalanzaDialog.js  # Diálogo para agregar balanzas
```

## Modelo de Datos

Colección `balanzas` en MongoDB:

```json
{
  "_id": "ObjectId",
  "ip": "172.28.3.250",
  "nombre": "Fileteado",
  "ultimaConexion": "2026-01-23T20:30:00Z",
  "estado": "ok"
}
```

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `_id` | ObjectId | Identificador único |
| `ip` | string | Dirección IP de la balanza |
| `nombre` | string | Nombre descriptivo |
| `ultimaConexion` | DateTime? | Timestamp del último HTTP 200 |
| `estado` | string | "ok" o "error" |

## API REST

Base URL: `http://localhost:5000/api`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/balanzas` | Lista todas las balanzas |
| GET | `/balanzas/{id}` | Obtiene una balanza por ID |
| POST | `/balanzas` | Crea una nueva balanza |
| PUT | `/balanzas/{id}` | Actualiza una balanza |
| DELETE | `/balanzas/{id}` | Elimina una balanza |

### Crear balanza

```bash
curl -X POST http://localhost:5000/api/balanzas \
  -H "Content-Type: application/json" \
  -d '{"nombre": "Fileteado", "ip": "172.28.3.250"}'
```

### Respuesta

```json
{
  "id": "6973dcff96217eb2242c2dac",
  "ip": "172.28.3.250",
  "nombre": "Fileteado",
  "ultimaConexion": null,
  "estado": "error"
}
```

## Funcionamiento del Monitoreo

El `MonitorService` es un `BackgroundService` que:

1. **Cada 5 segundos** obtiene la lista de balanzas de MongoDB
2. **Ejecuta hasta 50 requests en paralelo** usando `SemaphoreSlim`
3. Para cada balanza, hace `GET http://{IP}/balanza` con timeout de 3 segundos
4. **Si responde HTTP 200**:
   - Actualiza `estado = "ok"`
   - Actualiza `ultimaConexion` al timestamp actual
5. **Si falla o responde != 200**:
   - Actualiza `estado = "error"`
   - NO actualiza `ultimaConexion` (conserva el último valor exitoso)

## Configuración Docker

### Red

- **Nombre**: `balanzas_net`
- **Subnet**: `192.168.85.0/24`
- **Driver**: bridge

### Contenedores

| Servicio | IP | Puerto Host | Modo de Red |
|----------|-----|-------------|-------------|
| MongoDB | 192.168.85.10 | 27017 | balanzas_net |
| Backend | - | 5000 | host* |
| Frontend | 192.168.85.12 | 3000 | balanzas_net |

*El backend usa `network_mode: host` para poder acceder a las balanzas en redes externas (ej: 172.28.x.x).

### Volúmenes

- `mongodb_data`: Persistencia de datos de MongoDB

## Frontend

### Construcción dinámica de URL

El frontend construye la URL del backend dinámicamente usando `window.location.hostname`:

```javascript
const getApiUrl = () => {
  const hostname = window.location.hostname;
  return `http://${hostname}:5000/api`;
};
```

Esto permite que cuando se accede desde otro equipo (ej: `http://192.168.1.100:3000`), el frontend automáticamente use `http://192.168.1.100:5000/api` para el backend.

### Interfaz

- **Barra superior**: Título + botón "+" para agregar balanzas
- **Grid de cards**: Muestra todas las balanzas registradas
- **Card verde (success)**: Estado "ok" - balanza conectada
- **Card roja (danger)**: Estado "error" - balanza desconectada
- **Auto-refresh**: La lista se actualiza cada 3 segundos

## Instalación y Uso

### Requisitos

- Docker
- Docker Compose

### Levantar el sistema

```bash
# Clonar/copiar el proyecto
cd monitor

# Levantar todos los servicios
docker compose up -d --build

# Ver logs
docker compose logs -f

# Detener
docker compose down

# Detener y eliminar datos
docker compose down -v
```

### URLs de acceso

| Servicio | URL |
|----------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5000/api/balanzas |
| Swagger | http://localhost:5000/swagger |

## Pruebas

### Agregar balanza de prueba (funcional)

```bash
curl -X POST http://localhost:5000/api/balanzas \
  -H "Content-Type: application/json" \
  -d '{"nombre": "Fileteado", "ip": "172.28.3.250"}'
```

### Agregar balanza de prueba (error esperado)

```bash
curl -X POST http://localhost:5000/api/balanzas \
  -H "Content-Type: application/json" \
  -d '{"nombre": "PruebaError", "ip": "172.28.3.251"}'
```

### Verificar estados

```bash
curl http://localhost:5000/api/balanzas | python3 -m json.tool
```

## Notas Técnicas

1. **Timeout de balanzas**: 3 segundos para evitar bloqueos
2. **Concurrencia**: Máximo 50 requests simultáneos para soportar alto volumen
3. **Intervalo de polling**: 5 segundos entre ciclos de monitoreo
4. **CORS**: Habilitado para permitir acceso desde cualquier origen
5. **Persistencia**: Los datos sobreviven reinicios gracias al volumen Docker

## Logs

Ver logs del backend para diagnóstico:

```bash
docker logs balanzas_backend -f
```

Ejemplo de salida:
```
info: BalanzasMonitor.Services.MonitorService[0]
      Monitoreando 2 balanzas
warn: BalanzasMonitor.Services.MonitorService[0]
      Balanza PruebaError (172.28.3.251): Error - Connection refused
info: BalanzasMonitor.Services.MonitorService[0]
      Balanza Fileteado (172.28.3.250): OK
```
