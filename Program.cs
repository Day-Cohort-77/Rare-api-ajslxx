using RareAPI.Services;

using RareAPI.Models;

using RareAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<CommentServices>();
builder.Services.AddScoped<CategoriesServices>();
builder.Services.AddScoped<TagService>();


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



app.MapGet("/", () => "Welcome to Rare API!");


// Use CORS middleware
app.UseCors("AllowReactApp");

// Define API endpoints
app.MapGet("/", () => "Welcome to Rare API!");
app.MapTagEndpoints();
app.MapUserEndpoints();
app.MapCategoryEndpoints();
app.MapCommentEndpoints();
app.Run();





