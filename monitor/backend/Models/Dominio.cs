using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BalanzasMonitor.Models
{
    public class Dominio
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonElement("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [BsonElement("estado")]
        public int Estado { get; set; } = 1;

        [BsonElement("id_pais")]
        public string IdPais { get; set; } = string.Empty;

        [BsonElement("ldap_hosts")]
        public string LdapHosts { get; set; } = string.Empty;

        [BsonElement("ldap_port")]
        public int LdapPort { get; set; } = 389;

        [BsonElement("ldap_base_dn")]
        public string LdapBaseDn { get; set; } = string.Empty;

        [BsonElement("ldap_username")]
        public string? LdapUsername { get; set; }

        [BsonElement("ldap_password")]
        public string? LdapPassword { get; set; }

        [BsonElement("created_at")]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}