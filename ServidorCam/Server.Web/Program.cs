using System.Text.Json.Serialization;
using Server.Application;
using Server.Infrastructure;
using Server.Web.Helpers;
using Server.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Configurar para evitar ciclos de referencia
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Registrar servicios de Application e Infrastructure
builder.Services.AddApplicationObjects();
builder.Services.AddInfrastructureObjects(builder.Configuration);

// Registrar helpers
builder.Services.AddScoped<FileCacheHelper>();

// Background Service para sincronizar conexiones desde Server.Host vía Redis
builder.Services.AddHostedService<ConnectionSyncService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
