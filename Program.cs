
using RareAPI.Services;
using RareAPI.Models;
using RareAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<CommentServices>();

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
    await dbService.SeedDatabaseAsync();
}


app.MapGet("/", () => "Welcome to Rare API!");


app.MapGet("/users", async (DatabaseService dbService) =>
{
    return await dbService.GetAllUsersAsync();
});


app.MapCommentEndpoints();

app.Run();