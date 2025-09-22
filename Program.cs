using RareAPI.Services;
using RareAPI.Models;
using RareAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<PostSearchService>();
builder.Services.AddSingleton<UserServices>();
builder.Services.AddSingleton<PostServices>();
builder.Services.AddSingleton<CommentServices>();
builder.Services.AddSingleton<CategoriesServices>();
builder.Services.AddSingleton<TagService>();
builder.Services.AddSingleton<SubscriptionService>();
builder.Services.AddSingleton<ReactionServices>();


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


// Initialize database only if not in testing environment
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
        await dbService.InitializeDatabaseAsync();


        await dbService.SeedDatabaseAsync();
    }
}


app.UseCors("AllowReactApp");


app.MapGet("/", () => "Welcome to Rare API!");
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapPostEndpoints();
app.MapTagEndpoints();
app.MapCategoryEndpoints();
app.MapCommentEndpoints();
app.MapPostSearchEndpoints();
app.MapSubscriptionEndpoints();
app.MapReactionEndpoints();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
