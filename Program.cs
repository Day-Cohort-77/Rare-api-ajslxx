
using RareAPI.Services;
using RareAPI.Models;

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


app.MapGet("/comments", async (CommentServices commentService) =>
{
    return await commentService.GetAllCommentsAsync();
});


app.MapDelete("/comments/{id}", async (int id, CommentServices commentService) =>
{
    var deleted = await commentService.DeleteCommentAsync(id);
    if (deleted)
    {
        return Results.NoContent(); // 204 No Content - successful deletion
    }
    else
    {
        return Results.NotFound(); // 404 Not Found - comment doesn't exist
    }
});

app.Run();