using CarCompteur.Data;
using CarCompteur.Services;
using Microsoft.EntityFrameworkCore;
using CarCompteur.Dtos;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<ParkingDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("ParkingDb");
    options.UseNpgsql(cs);
});

builder.Services.AddScoped<IParkingService, ParkingService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CarCompteur API",
        Version = "v1",
        Description = "API de comptage de voitures pour parking"
    });
});

var app = builder.Build();

// Auto-migration au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParkingDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CarCompteur API v1");
    options.RoutePrefix = "swagger";
});

// POST /in
app.MapPost("/in", async (IParkingService service, CancellationToken ct) =>
{
    var current = await service.AddCarAsync(ct);
    return Results.Ok(new CountResponse(current));
})
.WithName("AddCar")
.WithTags("Parking");

// POST /out
app.MapPost("/out", async (IParkingService service, CancellationToken ct) =>
{
    try
    {
        var current = await service.RemoveCarAsync(ct);
        return Results.Ok(new CountResponse(current));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("RemoveCar")
.WithTags("Parking");

// GET /count
app.MapGet("/count", async (IParkingService service, CancellationToken ct) =>
{
    var current = await service.GetCurrentCountAsync(ct);
    return Results.Ok(new CountResponse(current));
})
.WithName("GetCurrentCount")
.WithTags("Parking");

// GET /stats/daily?date=2026-01-22
app.MapGet("/stats/daily", async (string date, IParkingService service, CancellationToken ct) =>
{
    if (!DateOnly.TryParse(date, out var d))
        return Results.BadRequest(new { error = "Paramètre 'date' invalide. Format attendu: YYYY-MM-DD" });

    var stats = await service.GetDailyStatsAsync(d, ct);
    return Results.Ok(stats);
})
.WithName("GetDailyStats")
.WithTags("Statistics");

app.Run();
