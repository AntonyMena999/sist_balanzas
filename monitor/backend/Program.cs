using MongoDB.Driver;
using BalanzasMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION")
    ?? "mongodb://localhost:27017";
var mongoClient = new MongoClient(mongoConnectionString);
var database = mongoClient.GetDatabase("balanzas_db");

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(database);

// HttpClient para las balanzas con timeout corto
builder.Services.AddHttpClient("BalanzaClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
});

// Background service para monitoreo
builder.Services.AddHostedService<MonitorService>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.MapControllers();

app.Run();
