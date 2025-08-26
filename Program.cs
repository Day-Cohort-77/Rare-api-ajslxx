using RareAPI.Services;
using RareAPI.Models;
using RareAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<PostServices>();
builder.Services.AddScoped<CommentServices>();
builder.Services.AddScoped<CategoriesServices>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<PostSearchService>();



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


using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();


    await dbService.SeedDatabaseAsync();
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
app.Run();






