using MongoDB.Driver;
using BalanzasMonitor.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// *** NUEVO: AuthService ***
builder.Services.AddScoped<AuthService>();

// *** NUEVO: JWT ***
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

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

// *** NUEVO ***
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();