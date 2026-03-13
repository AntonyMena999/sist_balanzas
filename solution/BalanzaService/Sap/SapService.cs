using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BalanzaService.Sap;

public class SapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SapService> _logger;
    private readonly string _wsUrl;
    private readonly string _username;
    private readonly string _password;

    public SapService(IConfiguration config, ILogger<SapService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _wsUrl = config["Sap:Url"] ?? throw new ArgumentException("Sap:Url no configurada");
        _username = config["Sap:Username"] ?? throw new ArgumentException("Sap:Username no configurado");
        _password = config["Sap:Password"] ?? throw new ArgumentException("Sap:Password no configurada");

        var authBytes = Encoding.ASCII.GetBytes($"{_username}:{_password}");
        var authBase64 = Convert.ToBase64String(authBytes);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authBase64);
    }

    public async Task<bool> EnviarPesoAsync(string ipAddress, double peso, CancellationToken cancellationToken = default)
    {
        if (peso <= 0)
        {
            return false;
        }

        try
        {
            var payload = new
            {
                ADDRESS = ipAddress,
                WEIGHT = peso
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Enviando a SAP: {Payload}", json);

            var response = await _httpClient.PostAsync(_wsUrl, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Respuesta SAP - Codigo: {StatusCode}, Body: {Body}",
                (int)response.StatusCode, responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SAP respondio con codigo de error: {StatusCode}", (int)response.StatusCode);
                return false;
            }

            var recibido = responseBody.Contains("RECEIVED", StringComparison.OrdinalIgnoreCase) ||
                           responseBody.Contains("RECIBIDO", StringComparison.OrdinalIgnoreCase);

            if (!recibido)
            {
                _logger.LogWarning("SAP no confirmo recepcion. Body: {Body}", responseBody);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar peso a SAP");
            return false;
        }
    }
}
