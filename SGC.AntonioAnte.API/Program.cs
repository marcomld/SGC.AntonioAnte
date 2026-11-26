using Microsoft.OpenApi.Models;
using SGC.AntonioAnte.API.Infrastructure;
using SGC.AntonioAnte.API.Services;
using SGC.AntonioAnte.Application; // Asegúrate de importar el namespace donde está el DependencyInjection
using SGC.AntonioAnte.Application.Common.Interfaces;
using SGC.AntonioAnte.Infrastructure;
using SGC.AntonioAnte.Infrastructure.Persistence;
using SGC.AntonioAnte.Infrastructure.Persistence.Seeders; // <-- Importante: Asegura este namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger con soporte para JWT Bearer
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SGC.AntonioAnte.API", Version = "v1" });

    // Definición del esquema de seguridad "Bearer"
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT en este formato: Bearer {tu_token}"
    });

    // Requerimiento de seguridad para que Swagger aplique el token
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Esto conecta tu DbContext e Identity configurados en Infrastructure
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor(); // Permite leer el contexto HTTP en cualquier lado
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); // Registra nuestro nuevo servicio
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaBlazorSGC", policy =>
    {
        policy.WithOrigins("https://localhost:7004") // El puerto exacto de tu cliente Blazor
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Permite el transporte seguro de cookies/cabeceras si fuera necesario
    });
});

var app = builder.Build();

// =========================================================================
// EJECUCIÓN AUTOMÁTICA DE SEMILLAS Y MIGRACIONES
// =========================================================================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 1. Semilla Geoespacial (Módulo 3 - GIS)
        await CatastroSeeder.SeedAsync(context, logger);

        // 2. 🔹 Semilla Financiera (Módulo 4 - Parámetros Económicos de Avalúos)
        await AvaluosSeeder.SeedAsync(context);

        logger.LogInformation("✅ Semillas de Catastro y Avalúos ejecutadas correctamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error crítico durante la inicialización de los Seeders.");
    }
}
// =========================================================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRouting();
app.UseCors("PoliticaBlazorSGC");
app.UseAuthorization();
app.MapControllers();

app.Run();