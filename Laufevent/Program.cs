using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Laufevent API",
        Version = "v1",
        Description = "API for managing Laufevent",
    });

    c.EnableAnnotations(); // Enable attributes for documentation
});

// Explicitly configure Kestrel to use HTTP only
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on HTTP (port 5000)
    options.ListenLocalhost(5000);  // Make sure it listens on HTTP
});

var app = builder.Build();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Laufevent API v1");
    c.RoutePrefix = "swagger"; // Swagger is available at /swagger
});

// Remove HTTPS redirection (we are using HTTP)
app.UseHttpsRedirection(); // COMMENT this line to disable HTTPS redirection
app.UseAuthorization();
app.MapControllers();

// Run the app (It will now be running over HTTP)
app.Run();
