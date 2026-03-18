using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BalanzasMonitor.Models
{
    public class Pais
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("estado")]
        public int Estado { get; set; } = 1;
    }
}