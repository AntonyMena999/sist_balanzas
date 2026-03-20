using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BalanzasMonitor.Models;

namespace BalanzasMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMongoCollection<Rol> _roles;

    public RolesController(IMongoClient client)
    {
        var database = client.GetDatabase("balanzas_db");
        _roles = database.GetCollection<Rol>("roles");
    }

    [HttpGet]
    public async Task<ActionResult<List<Rol>>> Get()
    {
        return await _roles.Find(_ => true).ToListAsync();
    }
}