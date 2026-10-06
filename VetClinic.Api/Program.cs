using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using VetClinic.Domain.Interfaces;
using VetClinic.Domain.Interfaces.Repositories;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Infrastructure.Data;
using VetClinic.Infrastructure.Repositories;
using VetClinic.Infrastructure.Security;
using VetClinic.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Fijar puerto local estándar http://localhost:5000 (RNF-07, RNF-08)
builder.WebHost.UseUrls("http://localhost:5000");

// Configuración de Inyección de Dependencias (Core & Infrastructure)
builder.Services.AddDbContext<VetClinicDbContext>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IPropietarioRepository, PropietarioRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IAtencionClinicaRepository, AtencionClinicaRepository>();
builder.Services.AddScoped<IInmunizacionRepository, InmunizacionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClinicaService, ClinicaService>();
builder.Services.AddScoped<IPdfExportService, QuestPdfExportService>();
builder.Services.AddSingleton<IExternalLauncherService, ExternalLauncherService>();

// Configuración de Controladores y Serialización JSON limpia
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configuración de CORS para desarrollo y estación monopuesto local
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return uri.Host == "localhost" || uri.Host == "127.0.0.1";
                }
                return false;
            })
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VetClinic Pro API",
        Version = "v1",
        Description = "API REST local para VetClinic Pro (Dres. Fabio y William) - Estación Monopuesto Localhost"
    });
});

var app = builder.Build();

// Inicializar base de datos SQLite embebida, triggers de inmutabilidad (Ley 576) y datos semilla
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbInitializer.InitializeAsync(dbContext, passwordHasher);
}

// Configurar el pipeline de solicitudes HTTP
app.UseCors("AllowLocalhost");

// Servir archivos estáticos de la SPA React en estación monopuesto local (RNF-07, RNF-08)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VetClinic Pro API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

// Fallback para rutas SPA React (monopuesto offline)
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
