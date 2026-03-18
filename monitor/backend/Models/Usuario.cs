using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BalanzasMonitor.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("rol_id")]
        [BsonRepresentation(BsonType.String)]
        public Guid RolId { get; set; }

        [BsonElement("dominio_id")]
        [BsonRepresentation(BsonType.String)]
        public Guid DominioId { get; set; }

        [BsonElement("estado")]
        public int Estado { get; set; } = 1;

        [BsonElement("pais")]
        public string Pais { get; set; } = string.Empty;

        [BsonElement("created_at")]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}