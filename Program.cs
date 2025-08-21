using RareAPI.Services;
using RareAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<PostServices>();

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
        await dbService.SeedDatabaseAsync();
}

// Define API endpoints
app.MapGet("/", () => "Welcome to Rare API!");

app.MapUserEndpoints();

app.MapPostEndpoints();
app.MapCategoryEndpoints();

app.Run();