using RareAPI.Services;
using RareAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Allow any origin for development
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
        await dbService.SeedDatabaseAsync();
}

// Use CORS middleware
app.UseCors("AllowReactApp");

// Define API endpoints
app.MapGet("/", () => "Welcome to Rare API!");

app.MapUserEndpoints();
app.MapTagEndpoints();

app.Run();
