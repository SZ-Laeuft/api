using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

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

// Build the app
var app = builder.Build();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Laufevent API v1");
    c.RoutePrefix = "swagger"; // Swagger is available at /swagger
});

// Force HTTPS redirection
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();