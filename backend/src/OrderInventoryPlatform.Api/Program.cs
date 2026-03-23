using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OrderInventoryPlatform.Api;
using OrderInventoryPlatform.Application;
using OrderInventoryPlatform.Infrastructure;
using OrderInventoryPlatform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddAzureForwardedHeaders();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "postgresql",
        tags: ["db", "ready"]);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
        });
    });
}

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    await using var seedScope = app.Services.CreateAsyncScope();
    var db = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DevelopmentDataSeeder.SeedAsync(db);

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAzureForwardedHeaders();

// Reverse proxy (Azure) or local HTTPS
var useForwardedHeaders = AzureHostingExtensions.ShouldUseForwardedHeaders(app.Environment, app.Configuration);
var runningInContainer =
    string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);

if (useForwardedHeaders || !runningInContainer)
{
    app.UseHttpsRedirection();
}

if (corsOrigins.Length > 0)
{
    app.UseCors();
}

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
});

app.Run();
