using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BalanzasMonitor.Models;

namespace BalanzasMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalanzasController : ControllerBase
{
    private readonly IMongoCollection<Balanza> _balanzas;
    private readonly ILogger<BalanzasController> _logger;

    public BalanzasController(IMongoDatabase database, ILogger<BalanzasController> logger)
    {
        _balanzas = database.GetCollection<Balanza>("balanzas");
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Balanza>>> GetAll()
    {
        var balanzas = await _balanzas.Find(_ => true).ToListAsync();
        return Ok(balanzas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Balanza>> GetById(string id)
    {
        var balanza = await _balanzas.Find(b => b.Id == id).FirstOrDefaultAsync();

        if (balanza == null)
        {
            return NotFound();
        }

        return Ok(balanza);
    }

    [HttpPost]
    public async Task<ActionResult<Balanza>> Create([FromBody] BalanzaCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Ip) || string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return BadRequest("IP y nombre son requeridos");
        }

        var existente = await _balanzas.Find(b => b.Ip == dto.Ip).FirstOrDefaultAsync();
        if (existente != null)
        {
            return Conflict($"Ya existe una balanza con la IP {dto.Ip}");
        }

        var balanza = new Balanza
        {
            Ip = dto.Ip,
            Nombre = dto.Nombre,
            Estado = "error",
            UltimaConexion = null,
            TiempoWarning = dto.TiempoWarning,
            TiempoDanger = dto.TiempoDanger
        };

        await _balanzas.InsertOneAsync(balanza);
        _logger.LogInformation("Balanza creada: {Nombre} ({Ip})", balanza.Nombre, balanza.Ip);

        return CreatedAtAction(nameof(GetById), new { id = balanza.Id }, balanza);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] BalanzaCreateDto dto)
    {
        var result = await _balanzas.UpdateOneAsync(
            b => b.Id == id,
            Builders<Balanza>.Update
                .Set(b => b.Ip, dto.Ip)
                .Set(b => b.Nombre, dto.Nombre)
                .Set(b => b.TiempoWarning, dto.TiempoWarning)
                .Set(b => b.TiempoDanger, dto.TiempoDanger));

        if (result.MatchedCount == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var result = await _balanzas.DeleteOneAsync(b => b.Id == id);

        if (result.DeletedCount == 0)
        {
            return NotFound();
        }

        _logger.LogInformation("Balanza eliminada: {Id}", id);
        return NoContent();
    }
}
