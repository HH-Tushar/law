
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using api.Data; // Required for [ApiController] and ControllerBase
using Microsoft.OpenApi.Models;

using ChatController.Abstract;
using ChatController.Implementation;
using UserController.Abstract;
using UserController.Implements; //


var builder = WebApplication.CreateBuilder(args);



// 1️⃣ Add CORS (allow all origins for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // required for SignalR
            .SetIsOriginAllowed(_ => true); // allow all origins
    });
});


// 2️⃣ Add SignalR
builder.Services.AddSignalR();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString));

// Controllers
builder.Services.AddControllers();

builder.Services.AddScoped<UserProfileAbstraction, UserProfileService>();
builder.Services.AddScoped<ChatControllerAbstraction, ChatControllerImplementation>();


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


// 3️⃣ Use CORS
app.UseCors("AllowAllOrigins");

// 4️⃣ Enable WebSockets
app.UseWebSockets();

// 5️⃣ Map your hub
app.MapHub<ChatHub>("/chathub");

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
