using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BalanzasMonitor.Models;

namespace BalanzasMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IMongoCollection<Usuario> _usuarios;

    public UsuariosController(IMongoClient client)
    {
        var database = client.GetDatabase("balanzas_db");
        _usuarios = database.GetCollection<Usuario>("usuarios");
    }

    [HttpGet]
    public async Task<ActionResult<List<Usuario>>> Get()
    {
        return await _usuarios.Find(_ => true).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> Get(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest("Formato de ID inválido");

        var usuario = await _usuarios.Find(u => u.Id == guid).FirstOrDefaultAsync();

        if (usuario == null) return NotFound();

        return usuario;
    }

    [HttpPost]
    public async Task<ActionResult<Usuario>> Post(Usuario usuario)
    {
        usuario.Id = Guid.NewGuid();
        usuario.CreatedAt = DateTime.UtcNow;
        
        await _usuarios.InsertOneAsync(usuario);

        return CreatedAtAction(nameof(Get), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, Usuario usuarioIn)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest("Formato de ID inválido");

        usuarioIn.Id = guid; // Asegurar que el ID coincida

        var result = await _usuarios.ReplaceOneAsync(u => u.Id == guid, usuarioIn);

        if (result.MatchedCount == 0) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest("Formato de ID inválido");

        var result = await _usuarios.DeleteOneAsync(u => u.Id == guid);

        if (result.DeletedCount == 0) return NotFound();

        return NoContent();
    }
}