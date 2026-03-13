using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BalanzasMonitor.Models;

public class Balanza
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("ip")]
    public string Ip { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("ultimaConexion")]
    public DateTime? UltimaConexion { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = "error";

    [BsonElement("ultimoPeso")]
    public double? UltimoPeso { get; set; }

    [BsonElement("ultimaMedicion")]
    public DateTime? UltimaMedicion { get; set; }

    [BsonElement("tiempoWarning")]
    public int TiempoWarning { get; set; } = 30;

    [BsonElement("tiempoDanger")]
    public int TiempoDanger { get; set; } = 60;
}

public class BalanzaCreateDto
{
    public string Ip { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int TiempoWarning { get; set; } = 30;
    public int TiempoDanger { get; set; } = 60;
}
