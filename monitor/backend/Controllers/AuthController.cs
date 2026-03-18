using BalanzasMonitor.Models;
using BalanzasMonitor.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BalanzasMonitor.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMongoDatabase _database;
        private readonly AuthService _authService;

        public AuthController(IMongoClient mongoClient, AuthService authService)
        {
            _database = mongoClient.GetDatabase("balanzas_db");
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            }

            var usuariosCollection = _database.GetCollection<Usuario>("usuarios");

            // 1. Buscar el usuario
            var usuario = await usuariosCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();

            if (usuario == null)
            {
                return Unauthorized(new { message = "Usuario no encontrado" });
            }

            // 2. Validar si está activo
            if (usuario.Estado == 0)
            {
                return Unauthorized(new { message = "Usuario inactivo" });
            }

            // 3. Buscar el Dominio
            var dominiosCollection = _database.GetCollection<Dominio>("dominios");
            var dominio = await dominiosCollection.Find(d => d.Id == usuario.DominioId).FirstOrDefaultAsync();

            if (dominio == null)
            {
                return Unauthorized(new { message = "Configuración de dominio no encontrada" });
            }

            // 4. Validar credenciales en AD
            bool credencialesValidas = _authService.ValidarCredencialesAD(
                dominio.LdapHosts,
                dominio.LdapPort,
                dominio.LdapBaseDn,
                request.Email,
                request.Password
            );

            if (!credencialesValidas)
            {
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }

            // 5. Buscar el Rol
            var rolesCollection = _database.GetCollection<Rol>("roles");
            var rol = await rolesCollection.Find(r => r.Id == usuario.RolId).FirstOrDefaultAsync();
            string nombreRol = rol?.Nombre ?? "Invitado";

            // 6. Generar JWT
            string codigoPais = usuario.Pais ?? "EC"; // Asumiendo default si es nulo
            var token = _authService.GenerarJwt(usuario, nombreRol, codigoPais);

            return Ok(new { token, email = usuario.Email, nombre = usuario.Nombre, rol = nombreRol, pais = codigoPais });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}