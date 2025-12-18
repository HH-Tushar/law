
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using api.Data; // Required for [ApiController] and ControllerBase
using Microsoft.OpenApi.Models;
using UserProfile.Abstract;
using UserProfile.Implements; //


var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString));

// Controllers
builder.Services.AddControllers();
builder.Services.AddScoped<UserProfileAbstraction, UserProfileImplementation>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer(); // Required for minimal APIs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Law AI API",
        Version = "v1"
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Law AI API v1");
        c.RoutePrefix = "swagger"; // Swagger UI served at /swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
