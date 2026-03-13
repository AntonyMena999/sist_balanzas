using BalanzaService.Measure;
using BalanzaService.Sap;
using BalanzaService.Web;
using Serilog;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

// Configurar ruta de logs junto al ejecutable
var exePath = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
var logsPath = Path.Combine(exePath, "logs");
Directory.CreateDirectory(logsPath);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logsPath, "balanza-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicacion...");
    Log.Information("Logs en: {LogsPath}", logsPath);

    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog
    builder.Host.UseSerilog();

    // Configurar como Windows Service
    builder.Host.UseWindowsService();

    // CORS - permitir cualquier origen
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    builder.Services.AddSingleton<BalanzaManager>();
    builder.Services.AddSingleton<SapService>();
    builder.Services.AddHostedService<BalanzaWorker>();

    var app = builder.Build();

    app.UseCors();
    app.MapBalanzaEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion fallo al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

public class BalanzaWorker : BackgroundService
{
    private readonly BalanzaManager _balanzaManager;
    private readonly SapService _sapService;
    private readonly ILogger<BalanzaWorker> _logger;
    private readonly int _intervaloMs;
    private string _ipAddress = string.Empty;

    public BalanzaWorker(
        BalanzaManager balanzaManager,
        SapService sapService,
        IConfiguration config,
        ILogger<BalanzaWorker> logger)
    {
        _balanzaManager = balanzaManager;
        _sapService = sapService;
        _logger = logger;
        _intervaloMs = config.GetValue<int>("Worker:IntervaloMs", 3000);
        _ipAddress = ObtenerIpValida();
    }

    private static bool EsIpValida(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;
        if (!IPAddress.TryParse(ip, out var addr)) return false;
        if (IPAddress.IsLoopback(addr)) return false;

        var bytes = addr.GetAddressBytes();
        // Rechazar 169.254.x.x (APIPA - sin DHCP)
        if (bytes.Length == 4 && bytes[0] == 169 && bytes[1] == 254) return false;

        return true;
    }

    private string ObtenerIpValida()
    {
        var ips = new List<string>();

        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var props = ni.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    var ip = addr.Address.ToString();
                    if (EsIpValida(ip))
                    {
                        ips.Add(ip);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enumerar interfaces de red");
        }

        if (ips.Count > 0)
        {
            if (ips.Count > 1)
            {
                _logger.LogInformation("IPs disponibles: {IPs}", string.Join(", ", ips));
            }
            return ips[0];
        }

        _logger.LogWarning("No se encontro una IP valida en las interfaces de red");
        return string.Empty;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("===========================================");
        _logger.LogInformation("Iniciando Servicio de Balanza");
        _logger.LogInformation("IP Local: {IP}", string.IsNullOrEmpty(_ipAddress) ? "(esperando red)" : _ipAddress);
        _logger.LogInformation("===========================================");

        try
        {
            await _balanzaManager.IniciarAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo iniciar el puerto serial. El servicio continuara solo con el servidor web.");
        }

        // Iniciar tarea de envío a SAP en segundo plano
        _ = Task.Run(async () => await EnviarASapLoopAsync(stoppingToken), stoppingToken);

        // Loop principal: solo lectura del puerto serial
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _balanzaManager.LeerPesoAsync();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ciclo de lectura");
            }

            await Task.Delay(_intervaloMs, stoppingToken);
        }
    }

    private async Task EnviarASapLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Refrescar IP si no tenemos una válida
                if (!EsIpValida(_ipAddress))
                {
                    var nuevaIp = ObtenerIpValida();
                    if (!string.IsNullOrEmpty(nuevaIp))
                    {
                        _ipAddress = nuevaIp;
                        _logger.LogInformation("IP valida obtenida: {IP}", _ipAddress);
                    }
                    else
                    {
                        _logger.LogWarning("Sin IP valida, esperando conexion de red...");
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }
                }

                if (_balanzaManager.PesoActual > 0)
                {
                    // Verificar que la IP sigue siendo válida justo antes de enviar
                    var ipActual = ObtenerIpValida();
                    if (!string.IsNullOrEmpty(ipActual) && ipActual != _ipAddress)
                    {
                        _logger.LogInformation("IP cambio de {IpAnterior} a {IpNueva}", _ipAddress, ipActual);
                        _ipAddress = ipActual;
                    }

                    var enviado = await _sapService.EnviarPesoAsync(_ipAddress, _balanzaManager.PesoActual, stoppingToken);
                    if (enviado)
                    {
                        _balanzaManager.ResetearPesoActual();
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ciclo de envio a SAP");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
