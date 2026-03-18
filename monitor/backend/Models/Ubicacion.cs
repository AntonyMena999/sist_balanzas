using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BalanzasMonitor.Models
{
    public class Ubicacion
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("pais_id")]
        public string PaisId { get; set; } = string.Empty;

        [BsonElement("estado")]
        public int Estado { get; set; } = 1;

        [BsonElement("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}