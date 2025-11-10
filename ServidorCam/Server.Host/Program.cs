using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Application;
using Server.Host.Services;
using Server.Infrastructure;
using Server.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

// Configuración
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())   
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Registrar servicios por capas (Clean Architecture)
builder.Services.AddInfrastructureObjects(builder.Configuration);  // Infrastructure: Repositories, DbContext, TcpServer
builder.Services.AddApplicationObjects();                          // Application: Services, Handlers

// Registrar el Background Service que maneja el TCP Server
builder.Services.AddHostedService<TcpServerHostedService>();
builder.Services.AddHostedService<VideoWorker>();

var host = builder.Build();

// Verify database connection
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ServerDbContext>();
        
        if (context.Database.CanConnect())
        {
            var dbName = builder.Configuration.GetConnectionString("DefaultConnection")
                ?.Split(';')
                .FirstOrDefault(s => s.Contains("Database="))
                ?.Split('=')[1];
            Console.WriteLine($"Database connection successful: {dbName}");
            
            // Apply pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Console.WriteLine("Applying pending migrations...");
                await context.Database.MigrateAsync();
                Console.WriteLine("Migrations applied successfully");
            }
        }
        else
        {
            Console.WriteLine("Database connection failed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
        Console.WriteLine("Continuing without database connection");
    }
}

Console.WriteLine("Server started. Press Ctrl+C to stop.");

// Ejecutar el host (esto iniciará el TcpServerHostedService automáticamente)
await host.RunAsync();
