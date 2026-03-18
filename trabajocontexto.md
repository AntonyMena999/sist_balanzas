# Contexto Maestro del Proyecto - Sistema de Balanzas Industriales

> **Archivo de Trabajo:** `trabajocontexto.md`
> **Estado:** En desarrollo activo / Fase de pruebas de integración.
> **Última Actualización:** 2026-03-18

## Descripción General

Sistema de dos componentes para gestionar balanzas industriales y visualizar su estado:

1.  **BalanzaService** (`solution/`): Servicio Windows (.NET 8) instalado en la PC conectada a la balanza. Lee el peso vía puerto serial, expone una API local y envía los datos a SAP.
2.  **Monitor** (`monitor/`): Dashboard web centralizado (React + .NET 8 + MongoDB) en Docker que monitorea en tiempo real la conectividad y estado de todas las balanzas en la red.

---

## Estructura del Proyecto

```text
balanzas/
├── solution/                          # Componente 1: BalanzaService (Servicio Windows)
│   ├── BalanzaService/
│   │   ├── Program.cs                 # Entry point + BalanzaWorker (background loops)
│   │   ├── Measure/                   # *** CONTROLLERS POR MODELO DE BALANZA ***
│   │   │   ├── IBalanza.cs            # Interfaz: PuedeParsear() + ParsearPeso()
│   │   │   ├── BalanzaLP7516.cs       # Parser: Ceres LP7516
│   │   │   ├── BalanzaMettler.cs      # Parser: Mettler-Toledo
│   │   │   └── BalanzaManager.cs      # Orquestador: puerto serial + lógica de lectura
│   │   ├── Sap/
│   │   │   └── SapService.cs          # Cliente HTTP para SAP REST API
│   │   └── Web/
│   │       └── BalanzaEndpoints.cs    # API Local: /balanza, /status, /health
│   ├── Installer/                     # Archivos WiX para generar MSI
│   └── scripts/                       # Scripts PowerShell de compilación e instalación
│
├── monitor/                           # Componente 2: Monitor Web (Docker)
│   ├── backend/                       # .NET 8 API
│   │   ├── Models/Balanza.cs          # Modelo MongoDB
│   │   ├── Controllers/BalanzasController.cs  # CRUD REST API
│   │   └── Services/MonitorService.cs # Background Service: Polling cada 5s
│   ├── frontend/                      # React 18
│   │   ├── src/components/BalanzaCard.js
│   │   └── src/services/api.js        # Cliente Axios
│   ├── docker-compose.yml             # MongoDB + Backend + Frontend
│   └── context.md
│
├── comunicacion.md                    # Documentación Protocolo serial LP7516
├── balanza_simulada.py                # Script Python para simular hardware
└── trabajocontexto.md                 # ESTE ARCHIVO (Memoria del proyecto)
```

---

## Componente 1: BalanzaService (Cliente Local)

Este servicio corre en la máquina física que tiene el cable serial conectado a la balanza.

### 1. Arquitectura y Lógica de Lectura (Serial)

El `BalanzaManager` utiliza un patrón Strategy para detectar el modelo de balanza automáticamente. Itera sobre una lista de implementaciones de `IBalanza` hasta que una reconoce la trama.

**Modelos Soportados:**
- **LP7516 (Ceres):** Trama `ST,GS,+ 0.75kg`.
- **Mettler-Toledo:** Inicio con "Date...".
- **Gross/Dix:** Formatos genéricos o fallback.

**Configuración Hardware (LP7516):**
- Baudrate: 9600, 8N1.
- Modo: Comando (C18=3) o Continuo.

### 2. Lógica de Envío a SAP (Regla de Negocio Crítica)

**Endpoint:** `http://sazsappodesa.kfc.com.ec:50000/RESTAdapter/SAPIntegration/SendBalanzaMeasures`

**Problema Resuelto (Loop):** Anteriormente, el peso se enviaba continuamente mientras estuviera en el buffer.
**Solución Actual:**
1.  `BalanzaManager` tiene una propiedad `HayLecturaNueva`.
2.  Al leer un peso > 0, se marca `HayLecturaNueva = true`.
3.  El `BalanzaWorker` (en `Program.cs`) verifica: `if (PesoActual > 0 && HayLecturaNueva)`.
4.  Solo si ambas son true, envía a SAP.
5.  Si SAP responde "RECEIVED" (HTTP 200), se llama a `ResetearPesoActual()`.
6.  `ResetearPesoActual()` pone `PesoActual = 0` y `HayLecturaNueva = false`.

### 3. API Local (Para el Monitor)

El servicio expone endpoints HTTP en el puerto 80 (configurable) para que el Monitor central pueda consultar su estado.

| Endpoint | Respuesta Ejemplo | Descripción |
|----------|-------------------|-------------|
| `/balanza` | `{"peso": "12.50"}` | Peso actual en tiempo real |
| `/status` | `{"peso": "...", "fecha": "..."}` | Estado completo para diagnóstico |
| `/health` | `"OK"` | Healthcheck simple |

### 4. Instalación (Windows)

- **Compilación:** Usar `solution/scripts/Publish.ps1`. Genera un `.exe` standalone.
- **Instalación:** Usar `solution/scripts/Install-Service.ps1`. Crea el servicio Windows "BalanzaService".
- **Configuración:** `appsettings.json` en la carpeta de instalación.

---

## Componente 2: Monitor (Dashboard Central)

Sistema dockerizado para visualizar el estado de todas las balanzas en planta.

### 1. Arquitectura

- **Base de Datos:** MongoDB (`balanzas_db`). Colección `balanzas`.
- **Backend:** .NET 8 API.
- **Frontend:** React 18 + Nginx.

### 2. Lógica de Monitoreo (MonitorService)

El backend ejecuta un `BackgroundService` cíclico:
1.  **Intervalo:** Cada 5 segundos.
2.  **Acción:** Obtiene la lista de IPs desde Mongo.
3.  **Polling:** Realiza un `GET http://{IP_BALANZA}/balanza` a cada una.
    - **Éxito (200):** Actualiza en Mongo `estado="ok"`, `ultimaConexion=Now`, `ultimoPeso`.
    - **Fallo:** Actualiza `estado="error"`. **NO** actualiza `ultimaConexion` (para saber cuándo fue la última vez que se vio).
4.  **Concurrencia:** Usa `SemaphoreSlim` para limitar peticiones simultáneas (máx 50).

### 3. Frontend (React)

- **Visualización:** Grid de tarjetas.
    - Verde: Estado OK. Muestra peso y última conexión.
    - Rojo: Estado Error. Muestra hace cuánto se perdió conexión.
- **Gestión:** Botón "+" para agregar nuevas balanzas (IP + Nombre).

### 4. Despliegue (Docker)

Ubicación: `monitor/`
Comando: `docker compose up -d --build`

**Red Docker (`balanzas_net`):**
- Subnet: `192.168.85.0/24`
- MongoDB: `192.168.85.10`
- Frontend: `192.168.85.12`
- Backend: `network_mode: host` (Crucial para que el contenedor pueda ver las IPs de las balanzas en la red física de la planta, ej: 172.28.x.x).

---

## Guía Rápida de Comandos

### BalanzaService (Windows)

```powershell
# 1. Publicar (Generar EXE)
cd solution/scripts
.\Publish.ps1

# 2. Instalar Servicio (Admin)
.\Install-Service.ps1

# 3. Ver Logs
Get-EventLog -LogName Application -Source BalanzaService -Newest 10
```

### Monitor (Docker Server)

```bash
cd monitor

# Levantar entorno
docker compose up -d --build

# Ver logs del backend
docker logs balanzas_backend -f
```

### Simulación

```bash
# Simular una balanza física en tu PC de desarrollo
python balanza_simulada.py
# Luego agregar tu IP local en el Monitor Web
```

---

## Historial de Cambios Recientes

### [2026-03-18] Fix Loop SAP
- **Problema:** BalanzaService enviaba el mismo peso infinitamente.
- **Causa:** La condición de envío solo miraba `Peso > 0`.
- **Cambio:** Se introdujo bandera `HayLecturaNueva` en `BalanzaManager`. El ciclo de envío en `Program.cs` ahora valida `if (Peso > 0 && HayLecturaNueva)`.

### [2026-03-18] Refactorización Monitor
- Separación completa Front/Back/DB.
- Implementación de persistencia en MongoDB.
- Frontend dinámico que permite agregar balanzas por IP.

---

## Notas para Desarrollo (Checklist)

1.  [ ] **Validar Fix SAP:** Probar con balanza física real (LP7516) que el peso se envíe una sola vez al estabilizarse.
2.  [ ] **Timeout Monitor:** Verificar que si una balanza se apaga, el monitor cambie a rojo en < 10 segundos.
3.  [ ] **Nuevos Modelos:** Si llega una balanza nueva, crear `BalanzaNuevoModelo.cs` implementando `IBalanza` y registrar en `BalanzaManager`.