using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BalanzasMonitor.Models;

namespace BalanzasMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DominiosController : ControllerBase
{
    private readonly IMongoCollection<Dominio> _dominios;

    public DominiosController(IMongoClient client)
    {
        var database = client.GetDatabase("balanzas_db");
        _dominios = database.GetCollection<Dominio>("dominios");
    }

    [HttpGet]
    public async Task<ActionResult<List<Dominio>>> Get()
    {
        return await _dominios.Find(_ => true).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Dominio>> Post(Dominio dominio)
    {
        dominio.Id = Guid.NewGuid();
        
        await _dominios.InsertOneAsync(dominio);

        return CreatedAtAction(nameof(Get), new { id = dominio.Id }, dominio);
    }
}