using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class TagEndpoints
  {
    public static void MapTagEndpoints(this WebApplication app)
    {
      // GET all tags
      app.MapGet("/tags", async (TagService tagService) =>
      {
        var tags = await tagService.GetAllTagsAsync();
        return Results.Ok(tags);
      });

      // POST new tag
      app.MapPost("/tags", async (TagService tagService, Tag newTag) =>
      {
        await tagService.AddTagAsync(newTag.Label);
        return Results.Created($"/tags/{newTag.Label}", newTag);
      });

      // DELETE tag by ID
      app.MapDelete("/tags/{id}", async (TagService tagService, int id) =>
      {
        await tagService.DeleteTagAsync(id);
        return Results.NoContent();
      });


      app.MapGet("/tags/{id}", async (TagService tagService, int id) =>
      {
        var tag = await tagService.GetTagByIdAsync(id);
        return Results.Ok(tag);
      });


      app.MapPut("/tags/{id}", async (TagService tagService, int id, Tag updatedTag) =>
      {
        await tagService.UpdateTagAsync(id, updatedTag.Label);
        return Results.Ok("Tag updated successfully");
      });

      // GET tags for a specific post
      app.MapGet("/posts/{postId}/tags", async (TagService tagService, int postId) =>
      {
        var tags = await tagService.GetPostTagsAsync(postId);
        return Results.Ok(tags);
      });

      // PUT (update) tags for a specific post
      app.MapPut("/posts/{postId}/tags", async (TagService tagService, int postId, List<int> tagIds) =>
      {
        await tagService.SavePostTagsAsync(postId, tagIds);
        return Results.Ok("Post tags updated successfully");
      });
    }
  }
}