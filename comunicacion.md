# Comunicación con Balanza LP7516

## Información General

| Parámetro | Valor |
|-----------|-------|
| Modelo | LP7516 Weighing Indicator |
| Interfaz | RS232 (via USB-Serial) |
| Puerto | `/dev/ttyUSB0` |
| Baudrate | 9600 bps |
| Data bits | 8 |
| Stop bits | 1 |
| Paridad | Ninguna |

## Configuración en la Balanza

Para habilitar la comunicación serial, configurar los siguientes parámetros en el menú de la balanza (presionar SET + ON/OFF):

### C18 - Modo de Interfaz Serial

| Valor | Modo | Descripción |
|-------|------|-------------|
| 0 | Sin envío | No transmite datos (default) |
| 1 | Big display | Formato para pantallas grandes |
| 2 | Print | Formato de impresión |
| **3** | **Comando** | **Responde a comandos (recomendado)** |
| 4 | Continuo | Envío continuo de datos |

### C19 - Velocidad de Transmisión (Baud Rate)

| Valor | Baudrate |
|-------|----------|
| 0 | 1200 bps |
| 1 | 2400 bps |
| 2 | 4800 bps |
| **3** | **9600 bps (default)** |

## Pinout Conector RS232 (DB9)

```
    5 4 3 2 1
     o o o o o
      o o o o
      9 8 7 6
```

| Pin | Señal | Descripción |
|-----|-------|-------------|
| 2 | TXD | Transmisión (balanza → PC) |
| 3 | RXD | Recepción (PC → balanza) |
| 5 | GND | Tierra |

## Comandos (Modo C18=3)

| Comando | Función | Descripción |
|---------|---------|-------------|
| `R` | Read | Leer peso actual |
| `Z` | Zero | Poner a cero |
| `T` | Tare | Realizar tara |
| `P` | Print | Imprimir peso |

## Formato de Respuesta

### Modo Comando (C18=3) - Respuesta a comando `R`

```
ST,GS,+   0.75kg\r\n
```

| Campo | Posición | Valores | Descripción |
|-------|----------|---------|-------------|
| Estado | 1-2 | `ST`, `US`, `OL` | Estable, Inestable, Sobrecarga |
| Modo | 4-5 | `GS`, `NT` | Gross (bruto), Net (neto) |
| Signo | 7 | `+`, `-` | Positivo o negativo |
| Peso | 8-14 | numérico | Valor del peso |
| Unidad | 15-16 | `kg` | Kilogramos |
| Terminador | - | `\r\n` | CR + LF |

### Modo Continuo (C18=4)

Formato similar enviado automáticamente cada ~100ms.

### Modo Big Display (C18=1)

```
STX SWA SWB SWC [12 bytes datos] CR CKS
```

Ver manual para decodificación de bits de estado.

## Ejemplo de Uso en Python

```python
import serial
import time

# Conexión
ser = serial.Serial('/dev/ttyUSB0', 9600, timeout=1)
time.sleep(0.3)

# Leer peso
ser.write(b'R')
respuesta = ser.readline().decode('utf-8').strip()
print(respuesta)  # ST,GS,+   0.75kg

# Zero
ser.write(b'Z')

# Tara
ser.write(b'T')

ser.close()
```

## Parseo de Respuesta

```python
import re

def parsear_peso(linea):
    """
    Parsea: ST,GS,+   0.75kg
    Retorna: {'estado': 'ST', 'modo': 'GS', 'signo': '+', 'peso': 0.75, 'unidad': 'kg'}
    """
    match = re.match(r'(\w+),(\w+),([+-])\s*([\d.]+)(\w+)', linea)
    if match:
        return {
            'estado': match.group(1),
            'modo': match.group(2),
            'signo': match.group(3),
            'peso': float(match.group(4)),
            'unidad': match.group(5)
        }
    return None
```

## Permisos en Linux

El usuario debe estar en el grupo `dialout`:

```bash
sudo usermod -a -G dialout $USER
# Cerrar sesión y volver a iniciar
```

## Errores Comunes

| Error | Causa | Solución |
|-------|-------|----------|
| No responde | C18=0 | Configurar C18=3 o C18=4 |
| Permission denied | Sin permisos | Agregar usuario a grupo `dialout` |
| `UUUUUU` en display | Sobrecarga o sin sensor | Verificar conexión load cell |
| `nnnnnn` en display | Mala calibración | Recalibrar balanza |

## Archivos del Proyecto

| Archivo | Descripción |
|---------|-------------|
| `leer_balanza.py` | Script de lectura de peso |
| `comunicacion.md` | Esta documentación |
| `Balanza.pdf` | Manual del fabricante |
