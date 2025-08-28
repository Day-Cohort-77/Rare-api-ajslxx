using RareAPI.Services;

namespace RareAPI.Endpoints;

public static class PostSearchEndpoints
{
    public static void MapPostSearchEndpoints(this WebApplication app)
    {
        app.MapGet("/posts/search", async (string q, PostSearchService searchService) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest("Query string cannot be empty.");

            var posts = await searchService.SearchAsync(q);
            return Results.Ok(posts);
        });
    }
}