using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class TagEndpoints
  {
    public static void MapTagEndpoints(this WebApplication app)
    {
      // GET all tags
      app.MapGet("/tags", async (DatabaseService db) =>
      {
        var tags = await db.GetAllTagsAsync();
        return Results.Ok(tags);
      });

      // POST new tag
      app.MapPost("/tags", async (DatabaseService db, Tag newTag) =>
      {
        await db.AddTagAsync(newTag.Label);
        return Results.Created($"/tags/{newTag.Label}", newTag);
      });
      // DELETE tag by ID
      app.MapDelete("/tags/{id}", async (DatabaseService db, int id) =>
{
  await db.DeleteTagAsync(id);
  return Results.NoContent();
});
      // Add these endpoints to your existing TagEndpoints class

      // GET single tag by id
      app.MapGet("/tags/{id}", async (DatabaseService db, int id) =>
      {
        var tag = await db.GetTagByIdAsync(id);
        return Results.Ok(tag);
      });

      // PUT update tag by id
      app.MapPut("/tags/{id}", async (DatabaseService db, int id, Tag updatedTag) =>
      {
        await db.UpdateTagAsync(id, updatedTag.Label);
        return Results.Ok($"Tag updated successfully");
      });
      
    }
  }
}
