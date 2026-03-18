using BalanzasMonitor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Novell.Directory.Ldap;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BalanzasMonitor.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Valida las credenciales contra el servidor LDAP/Active Directory.
        /// </summary>
        /// <param name="ldapHost">Host o IP del servidor LDAP (viene de Dominio).</param>
        /// <param name="ldapPort">Puerto (ej: 389).</param>
        /// <param name="ldapBaseDn">Base DN (ej: DC=kfc,DC=ec).</param>
        /// <param name="email">Email del usuario para autenticar (User Principal Name).</param>
        /// <param name="password">Contraseña del usuario.</param>
        /// <returns>True si las credenciales son válidas.</returns>
        public bool ValidarCredencialesAD(string ldapHost, int ldapPort, string ldapBaseDn, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(ldapHost) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            try
            {
                using (var connection = new LdapConnection())
                {
                    // 1. Conectar al servidor
                    connection.Connect(ldapHost, ldapPort);

                    // 2. Bind (Autenticación)
                    // En Active Directory moderno, se puede hacer bind directo con el email (user@dominio.com)
                    // sin necesidad de buscar primero el DN del usuario.
                    connection.Bind(email, password);

                    return connection.Bound;
                }
            }
            catch (LdapException)
            {
                // La autenticación falló o no se pudo conectar
                return false;
            }
            catch (Exception)
            {
                // Cualquier otro error no controlado
                return false;
            }
        }

        /// <summary>
        /// Genera un token JWT con la expiración y claims configurados.
        /// </summary>
        /// <param name="usuario">Objeto Usuario autenticado.</param>
        /// <param name="nombreRol">Nombre del rol (ej: admin, soporte).</param>
        /// <param name="codigoPais">Código de país (ej: EC).</param>
        /// <returns>String del token JWT.</returns>
        public string GenerarJwt(Usuario usuario, string nombreRol, string codigoPais)
        {
            var secretKey = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("La configuración Jwt:Secret no está definida en appsettings.json");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("userId", usuario.Id.ToString()),
                new Claim("email", usuario.Email ?? ""),
                new Claim("rol", nombreRol ?? ""),
                new Claim("pais", codigoPais ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: null, // Se puede configurar si es necesario
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
