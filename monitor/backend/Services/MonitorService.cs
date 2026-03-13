using MongoDB.Driver;
using BalanzasMonitor.Models;
using System.Text.Json;

namespace BalanzasMonitor.Services;

public class MonitorService : BackgroundService
{
    private readonly IMongoCollection<Balanza> _balanzas;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MonitorService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(5);
    private readonly int _maxConcurrency = 50;

    public MonitorService(
        IMongoDatabase database,
        IHttpClientFactory httpClientFactory,
        ILogger<MonitorService> logger)
    {
        _balanzas = database.GetCollection<Balanza>("balanzas");
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitor de balanzas iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorearBalanzasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo de monitoreo");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task MonitorearBalanzasAsync(CancellationToken ct)
    {
        var balanzas = await _balanzas.Find(_ => true).ToListAsync(ct);

        if (balanzas.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Monitoreando {Count} balanzas", balanzas.Count);

        var semaphore = new SemaphoreSlim(_maxConcurrency);
        var tasks = balanzas.Select(async balanza =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await VerificarBalanzaAsync(balanza, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task VerificarBalanzaAsync(Balanza balanza, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("BalanzaClient");
        var url = $"http://{balanza.Ip}/status";

        try
        {
            var response = await client.GetAsync(url, ct);

            var update = Builders<Balanza>.Update
                .Set(b => b.Estado, response.IsSuccessStatusCode ? "ok" : "error");

            if (response.IsSuccessStatusCode)
            {
                update = update.Set(b => b.UltimaConexion, DateTime.UtcNow);

                // Leer el peso y fecha del response
                var content = await response.Content.ReadAsStringAsync(ct);
                var (peso, fecha) = ParsearStatus(content);

                if (peso.HasValue)
                {
                    update = update.Set(b => b.UltimoPeso, peso.Value);
                }

                if (fecha.HasValue)
                {
                    update = update.Set(b => b.UltimaMedicion, fecha.Value);
                }

                _logger.LogDebug("Balanza {Nombre} ({Ip}): OK - Peso: {Peso}, Fecha: {Fecha}", 
                    balanza.Nombre, balanza.Ip, peso, fecha);
            }
            else
            {
                _logger.LogWarning("Balanza {Nombre} ({Ip}): HTTP {Status}",
                    balanza.Nombre, balanza.Ip, (int)response.StatusCode);
            }

            await _balanzas.UpdateOneAsync(
                b => b.Id == balanza.Id,
                update,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Balanza {Nombre} ({Ip}): Error - {Message}",
                balanza.Nombre, balanza.Ip, ex.Message);

            await _balanzas.UpdateOneAsync(
                b => b.Id == balanza.Id,
                Builders<Balanza>.Update.Set(b => b.Estado, "error"),
                cancellationToken: ct);
        }
    }

    private (double?, DateTime?) ParsearStatus(string content)
    {
        double? peso = null;
        DateTime? fecha = null;

        try
        {
            using var doc = JsonDocument.Parse(content);
            
            if (doc.RootElement.TryGetProperty("peso", out var pesoElement))
            {
                var pesoStr = pesoElement.GetString()?.Replace(',', '.');
                if (double.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var pesoValue))
                {
                    peso = pesoValue;
                }
            }

            if (doc.RootElement.TryGetProperty("fecha", out var fechaElement))
            {
                var fechaStr = fechaElement.GetString();
                if (!string.IsNullOrEmpty(fechaStr) && DateTime.TryParse(fechaStr, out var fechaValue))
                {
                    // La fecha viene en hora local del dispositivo, usar zona horaria del contenedor (TZ)
                    var localZone = TimeZoneInfo.Local;
                    var fechaLocal = DateTime.SpecifyKind(fechaValue, DateTimeKind.Unspecified);
                    fecha = TimeZoneInfo.ConvertTimeToUtc(fechaLocal, localZone);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error parseando status: {Message}", ex.Message);
        }

        return (peso, fecha);
    }
}
