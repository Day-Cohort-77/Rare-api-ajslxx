using RareAPI.Services;

using RareAPI.Models;

using RareAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<CommentServices>();
builder.Services.AddScoped<CategoriesServices>();

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
    await dbService.SeedDatabaseAsync();
}


app.MapGet("/", () => "Welcome to Rare API!");


app.MapUserEndpoints();
app.MapCategoryEndpoints();
app.MapCommentEndpoints();
app.Run();





