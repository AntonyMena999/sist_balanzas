# Contexto Completo del Proyecto - Sistema de Balanzas Industriales

## Descripcion General

Sistema de dos componentes para gestionar balanzas industriales:

1. **BalanzaService** (`solution/`): Servicio Windows (.NET 8) que lee peso desde balanzas via puerto serial y lo envia a SAP.
2. **Monitor** (`monitor/`): Dashboard web (React + .NET 8 + MongoDB) que monitorea en tiempo real la conectividad y estado de las balanzas en red.

---

## Estructura del Proyecto

```
balanzas/
├── solution/                          # BalanzaService (Servicio Windows)
│   ├── BalanzaService/
│   │   ├── Program.cs                 # Entry point + BalanzaWorker (background loops)
│   │   ├── BalanzaService.csproj      # .NET 8, win-x64, self-contained
│   │   ├── appsettings.json           # Config: serial, SAP, intervalos
│   │   ├── appsettings.Production.json
│   │   ├── Measure/                   # *** CONTROLLERS POR MODELO DE BALANZA ***
│   │   │   ├── IBalanza.cs            # Interfaz: PuedeParsesr() + ParsearPeso()
│   │   │   ├── BalanzaLP7516.cs       # Ceres LP7516
│   │   │   ├── BalanzaMettler.cs      # Mettler-Toledo
│   │   │   ├── BalanzaGross.cs        # Formato GROSS generico
│   │   │   ├── BalanzaDix.cs          # Fallback/catch-all
│   │   │   └── BalanzaManager.cs      # Orquestador: puerto serial + deteccion
│   │   ├── Sap/
│   │   │   └── SapService.cs          # Cliente HTTP para SAP REST API
│   │   └── Web/
│   │       └── BalanzaEndpoints.cs    # Endpoints: /balanza, /status, /health
│   ├── Installer/                     # Archivos WiX para generar MSI
│   │   └── BalanzaServiceLinux.wxs
│   ├── Dockerfile
│   └── docker-compose.yml
│
├── monitor/
│   ├── backend/                       # .NET 8 API
│   │   ├── Program.cs
│   │   ├── Models/Balanza.cs          # Modelo MongoDB
│   │   ├── Controllers/BalanzasController.cs  # CRUD REST API
│   │   └── Services/MonitorService.cs # Polling cada 5s a las balanzas
│   ├── frontend/                      # React 18
│   │   ├── src/App.js                 # Componente principal, auto-refresh 3s
│   │   ├── src/components/BalanzaCard.js
│   │   ├── src/components/AddBalanzaDialog.js
│   │   └── src/services/api.js        # Cliente Axios
│   ├── docker-compose.yml             # MongoDB + Backend + Frontend
│   └── context.md
│
├── Desplegar/                         # Artefactos de despliegue
│   ├── BalanzaService.msi
│   └── BalanzaService.zip
├── logs/
├── comunicacion.md                    # Protocolo serial LP7516
├── compilacion.md                     # Instrucciones de compilacion
├── contexto.md                        # ESTE ARCHIVO
└── README.md
```

---

## IMPORTANTE: Controllers por Modelo de Balanza

**Cada modelo de balanza tiene su propio controller (parser) en `solution/BalanzaService/Measure/`.**

Todos implementan la interfaz `IBalanza`:

```csharp
public interface IBalanza
{
    bool PuedeParsesr(string datos);   // Detecta si los datos son de este modelo
    double ParsearPeso(string datos);   // Extrae el peso numerico
}
```

### Controllers existentes:

| Controller | Archivo | Modelo | Reconocimiento | Formato ejemplo |
|------------|---------|--------|----------------|-----------------|
| `BalanzaLP7516` | `BalanzaLP7516.cs` | Ceres LP7516 | Formato `ST,GS,+` o STX byte o modo continuo | `ST,GS,+   0.75kg` |
| `BalanzaMettler` | `BalanzaMettler.cs` | Mettler-Toledo | Empieza con "Date" | `Date...Gross 2.45 kg...Tare...` |
| `BalanzaGross` | `BalanzaGross.cs` | Formato GROSS generico | Empieza con "GROSS" | `GROSS    6.5 kg` |
| `BalanzaDix` | `BalanzaDix.cs` | Fallback/catch-all | Contiene "kg" y no matchea otros | `0 3.75 kg` |

### Flujo de deteccion (en BalanzaManager):

```
Dato serial recibido
  → BalanzaLP7516.PuedeParsesr() → si match → ParsearPeso() → return peso
  → BalanzaMettler.PuedeParsesr() → si match → ParsearPeso() → return peso
  → BalanzaGross.PuedeParsesr()  → si match → ParsearPeso() → return peso
  → BalanzaDix.PuedeParsesr()    → si match → ParsearPeso() → return peso (fallback)
```

### REGLA: Cuando haya un tipo nuevo de balanza, se debe:

1. Crear un nuevo archivo `Balanza{NombreModelo}.cs` en `solution/BalanzaService/Measure/`
2. Implementar la interfaz `IBalanza` con `PuedeParsesr()` y `ParsearPeso()`
3. Registrar la nueva instancia en `BalanzaManager.cs` (lista de parsers)
4. El `PuedeParsesr()` debe identificar el formato unico de la trama serial del modelo
5. El `ParsearPeso()` debe extraer el valor numerico del peso

---

## Arquitectura BalanzaService

### Background Worker (Program.cs)

Dos loops asincrono ejecutandose en paralelo:

1. **Loop Principal** (cada 3s configurable via `Worker:IntervaloMs`):
   - Llama `BalanzaManager.LeerPesoAsync()`
   - Lee buffer del puerto serial
   - Intenta parsear con cada controller de balanza

2. **Loop SAP** (cada 1s):
   - Si `PesoActual > 0`, envia a SAP
   - Detecta IP local valida (excluye loopback y APIPA 169.254.x.x)
   - POST a SAP con `{ADDRESS: ip, WEIGHT: peso}`
   - Si SAP responde 200 + "RECEIVED"/"RECIBIDO" → resetea peso

### SapService

- **URL:** `http://sazsappodesa.kfc.com.ec:50000/RESTAdapter/SAPIntegration/SendBalanzaMeasures`
- **Auth:** Basic Auth (base64)
- **Payload:** `{"ADDRESS": "192.168.1.100", "WEIGHT": 12.5}`
- **Validacion:** HTTP 2xx + body contiene "RECEIVED" o "RECIBIDO"

### Endpoints HTTP (BalanzaEndpoints.cs)

| Endpoint | Metodo | Respuesta |
|----------|--------|-----------|
| `/balanza` | GET | `{"peso": "12.50"}` |
| `/status` | GET | `{"peso": "12.50", "fecha": "2026-01-23T14:30:00"}` |
| `/health` | GET | `"OK"` |

### Configuracion (appsettings.json)

```json
{
  "Serial": { "Puerto": "COM2", "BaudRate": 9600 },
  "Sap": {
    "Url": "http://sazsappodesa.kfc.com.ec:50000/RESTAdapter/SAPIntegration/SendBalanzaMeasures",
    "Username": "USER_IF_Bala",
    "Password": "inicio2020"
  },
  "Worker": { "IntervaloMs": 3000 },
  "Kestrel": { "Endpoints": { "Http": { "Url": "http://0.0.0.0:80" } } }
}
```

---

## Arquitectura Monitor

### Backend (.NET 8 + MongoDB)

**API REST** (`/api/balanzas`):

| Metodo | Endpoint | Funcion |
|--------|----------|---------|
| GET | `/` | Listar todas las balanzas |
| GET | `/{id}` | Obtener una balanza |
| POST | `/` | Crear balanza (requiere ip + nombre) |
| PUT | `/{id}` | Actualizar balanza |
| DELETE | `/{id}` | Eliminar balanza |

**MonitorService** (background):
- Cada 5 segundos hace `GET http://{IP}/status` a cada balanza registrada
- Maximo 50 requests simultaneos (SemaphoreSlim)
- Timeout 3 segundos por request
- Si responde 200: `estado="ok"`, actualiza `ultimaConexion`, guarda peso y fecha
- Si falla: `estado="error"`, mantiene `ultimaConexion` anterior (sticky)

**Modelo MongoDB** (coleccion `balanzas`, DB `balanzas_db`):

```json
{
  "ip": "172.28.3.250",
  "nombre": "Fileteado",
  "ultimaConexion": "2026-01-23T20:30:00Z",
  "estado": "ok",
  "ultimoPeso": 12.5,
  "ultimaMedicion": "2026-01-23T20:29:00Z",
  "tiempoWarning": 30,
  "tiempoDanger": 60
}
```

### Frontend (React 18)

- **App.js:** Fetch cada 3s, grid de cards, estadisticas (total/conectadas/desconectadas)
- **BalanzaCard.js:** Card verde (ok) o roja (error), badges por antiguedad de medicion
- **AddBalanzaDialog.js:** Modal para crear/editar con validacion
- **api.js:** Axios con baseURL dinamico `http://{hostname}:5000/api`

### Docker Compose (monitor/docker-compose.yml)

| Servicio | IP/Puerto | Red |
|----------|-----------|-----|
| MongoDB | 192.168.85.10:27017 | balanzas_net |
| Backend | host network, puerto 5000 | host (para acceder a red de balanzas) |
| Frontend | 192.168.85.12:3000 | balanzas_net |

---

## Protocolo Serial LP7516

- **Interfaz:** RS232 via adaptador USB-Serial
- **Puerto:** `/dev/ttyUSB0` (Linux) / `COM2` (Windows)
- **Baudrate:** 9600, 8N1, sin handshake
- **Modos (parametro C18):**
  - 0: Sin transmision
  - 1: Big Display (binario)
  - 3: Comando (recomendado) - responde a R/Z/T/P
  - 4: Continuo - envia peso cada ~100ms
- **Formato respuesta (modo comando):** `ST,GS,+   0.75kg\r\n`
  - ST=Stable/US=Unstable/OL=Overload
  - GS=Gross/NT=Net

---

## Compilacion

### Requisitos (Ubuntu)

```bash
sudo apt install dotnet-sdk-8.0
sudo apt install msitools wixl
```

### Proceso de Compilacion

#### 1. Compilar el proyecto para Windows x64

```bash
cd /home/lsalazar/Proyectos/balanzas/solution

dotnet publish BalanzaService/BalanzaService.csproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    -o Installer/publish
```

#### 2. Copiar appsettings.json al directorio publish

```bash
cp BalanzaService/appsettings.json Installer/publish/
```

#### 3. Generar el MSI con wixl

```bash
cd Installer
wixl -v -o BalanzaService.msi BalanzaServiceLinux.wxs
```

### Archivos Generados

| Archivo | Ubicacion | Descripcion |
|---------|-----------|-------------|
| `BalanzaService.msi` | `Installer/` | Instalador MSI (~42MB) |
| `BalanzaService.exe` | `Installer/publish/` | Ejecutable del servicio |
| `appsettings.json` | `Installer/publish/` | Configuracion del servicio |

### Copiar a USB

```bash
lsblk -o NAME,SIZE,TYPE,MOUNTPOINT | grep media
cp Installer/BalanzaService.msi /media/lsalazar/LUCHOX/
```

### Archivos WXS

- `BalanzaServiceLinux.wxs` - Formato WiX v3, compatible con `wixl` (Linux) **← USAR ESTE**
- `Package.wxs` - Formato WiX v5, compatible con `wix` (Windows)
- `BalanzaService.wxs` - Formato WiX v3 antiguo (no usar)

### Script Completo

```bash
#!/bin/bash
cd /home/lsalazar/Proyectos/balanzas/solution

dotnet publish BalanzaService/BalanzaService.csproj \
    -c Release -r win-x64 --self-contained \
    -o Installer/publish

cp BalanzaService/appsettings.json Installer/publish/

cd Installer
wixl -v -o BalanzaService.msi BalanzaServiceLinux.wxs

echo "MSI generado: $(ls -lh BalanzaService.msi)"
```

### Notas de Instalacion

- Instala en `C:\Program Files\BalanzaService\`
- Se registra como servicio "BalanzaService" con inicio automatico
- Inicia automaticamente tras instalacion
- Para cambiar puerto COM: editar `appsettings.json` en carpeta de instalacion y reiniciar servicio
- Para produccion: usar `appsettings.Production.json` con credenciales de produccion

---

## Despliegue Monitor (Docker)

```bash
cd /home/lsalazar/Proyectos/balanzas/monitor
docker compose up -d --build

# URLs:
# Frontend: http://localhost:3000
# Backend:  http://localhost:5000/api/balanzas
# Swagger:  http://localhost:5000/swagger
# MongoDB:  localhost:27017
```

---

## Stack Tecnologico

| Componente | Tecnologia |
|------------|-----------|
| BalanzaService | .NET 8, ASP.NET Core, Serilog, System.IO.Ports |
| Monitor Backend | .NET 8, MongoDB.Driver 2.23.1, Swagger |
| Monitor Frontend | React 18, Axios, Nginx |
| Base de datos | MongoDB 7 |
| Contenedores | Docker Compose |
| Instalador | WiX v3 (wixl en Linux) |
| Logging | Serilog (rolling file, 7 dias retencion) |
