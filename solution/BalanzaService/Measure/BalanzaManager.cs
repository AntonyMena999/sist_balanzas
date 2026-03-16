using System.IO.Ports;

namespace BalanzaService.Measure;

public class BalanzaManager : IDisposable
{
    private SerialPort? _serialPort;
    private readonly List<IBalanza> _balanzas;
    private readonly ILogger<BalanzaManager> _logger;
    private readonly string _puerto;
    private readonly int _baudRate;
    private bool _puertoAbierto = false;

    public double UltimoPeso { get; private set; }
    public double PesoActual { get; private set; }
    public DateTime? UltimaFechaMedicion { get; private set; }
    public string? UltimaBalanzaDetectada { get; private set; }
    public bool HayLecturaNueva { get; private set; } = false;

    public BalanzaManager(IConfiguration config, ILogger<BalanzaManager> logger)
    {
        _logger = logger;

        _puerto = config["Serial:Puerto"] ?? "COM2";
        _baudRate = config.GetValue<int>("Serial:BaudRate", 9600);

        _logger.LogInformation("Configuracion Serial: Puerto={Puerto}, BaudRate={BaudRate}", _puerto, _baudRate);

        _balanzas = new List<IBalanza>
        {
            new BalanzaLP7516(),
            new BalanzaMettler(),
            new BalanzaGross(),
            new BalanzaDix()
        };

        // Listar puertos disponibles
        var puertosDisponibles = SerialPort.GetPortNames();
        _logger.LogInformation("Puertos COM disponibles: {Puertos}", string.Join(", ", puertosDisponibles));
    }

    public async Task IniciarAsync()
    {
        try
        {
            _serialPort = new SerialPort(_puerto, _baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 2000,
                WriteTimeout = 1000,
                DtrEnable = false,
                RtsEnable = false,
                Handshake = Handshake.None,
                NewLine = "\r\n"
            };

            _serialPort.Open();
            _puertoAbierto = true;
            _logger.LogInformation("Puerto serial abierto exitosamente: {Puerto} @ {BaudRate} bps", _puerto, _baudRate);

            // Limpiar buffers
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            await Task.Delay(500); // Dar tiempo al puerto para estabilizarse
        }
        catch (Exception ex)
        {
            _puertoAbierto = false;
            _logger.LogError(ex, "Error al abrir puerto serial {Puerto}", _puerto);
            throw;
        }
    }

    private async Task ReabrirPuertoAsync()
    {
        try
        {
            _logger.LogWarning("Intentando reabrir puerto serial...");
            _serialPort?.Close();
            _serialPort?.Dispose();
            await Task.Delay(1000);
            await IniciarAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reabrir puerto serial");
            _puertoAbierto = false;
        }
    }

    public async Task LeerPesoAsync()
    {
        try
        {
            if (_serialPort == null || !_puertoAbierto || !_serialPort.IsOpen)
            {
                _logger.LogWarning("Puerto serial no esta abierto, intentando reconectar...");
                await ReabrirPuertoAsync();
                return;
            }

            // Leer datos pendientes en el buffer (enviados por la balanza al presionar SET)
            var bytesDisponibles = _serialPort.BytesToRead;

            if (bytesDisponibles == 0)
            {
                return; // No hay datos pendientes, esperar siguiente ciclo
            }

            _logger.LogDebug("Bytes disponibles en buffer: {Bytes}", bytesDisponibles);

            string data = "";

            try
            {
                // Leer todos los datos disponibles
                data = _serialPort.ReadExisting();
            }
            catch (TimeoutException)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                return;
            }

            // Procesar cada linea recibida (puede haber varias)
            var lineas = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var linea in lineas)
            {
                var lineaLimpia = linea.Trim();
                if (string.IsNullOrWhiteSpace(lineaLimpia))
                    continue;

                _logger.LogInformation("Datos recibidos de balanza: [{Data}]", lineaLimpia);

                foreach (var balanza in _balanzas)
                {
                    if (balanza.PuedeParsesr(lineaLimpia))
                    {
                        var peso = balanza.ParsearPeso(lineaLimpia);
                        if (peso.HasValue)
                        {
                            PesoActual = peso.Value;
                            UltimoPeso = peso.Value;
                            UltimaFechaMedicion = DateTime.Now;
                            UltimaBalanzaDetectada = balanza.Nombre;
                            HayLecturaNueva = true;
                            _logger.LogInformation(">>> Balanza {Nombre}: Peso = {Peso} kg <<<", balanza.Nombre, peso.Value);
                            return;
                        }
                        else
                        {
                            _logger.LogWarning("Balanza {Nombre} no pudo parsear: [{Data}]", balanza.Nombre, lineaLimpia);
                        }
                    }
                }
            }
        }
        catch (TimeoutException)
        {
            // Normal, no hay datos
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error de I/O en puerto serial, se intentara reconectar");
            _puertoAbierto = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer peso");
        }
    }

    public void ResetearPesoActual()
    {
        PesoActual = 0.0;
        HayLecturaNueva = false;
    }

    public void Dispose()
    {
        try
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar puerto serial");
        }
    }
}
