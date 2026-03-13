using BalanzaService.Measure;

namespace BalanzaService.Web;

public static class BalanzaEndpoints
{
    public static void MapBalanzaEndpoints(this WebApplication app)
    {
        // Endpoint legacy - solo retorna el peso
        app.MapGet("/balanza", (BalanzaManager balanzaManager) =>
        {
            var peso = balanzaManager.UltimoPeso.ToString("F2");
            return Results.Json(new { peso });
        });

        // Nuevo endpoint - retorna peso y fecha de medición
        app.MapGet("/status", (BalanzaManager balanzaManager) =>
        {
            var peso = balanzaManager.UltimoPeso.ToString("F2");
            var fecha = balanzaManager.UltimaFechaMedicion?.ToString("yyyy-MM-ddTHH:mm:ss");
            return Results.Json(new { peso, fecha });
        });

        app.MapGet("/health", () => Results.Ok("OK"));
    }
}
