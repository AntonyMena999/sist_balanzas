using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BalanzasMonitor.Models
{
    public class Rol
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("descripcion")]
        public string? Descripcion { get; set; }

        [BsonElement("permisos")]
        public string Permisos { get; set; } = string.Empty;

        [BsonElement("estado")]
        public int Estado { get; set; } = 1;

        [BsonElement("created_at")]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}