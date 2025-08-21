using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class PostEndpoints
  {
    public static void MapPostEndpoints(this WebApplication app)
    {
      app.MapGet("/posts", async (DatabaseService db) =>
      {
        var users = await db.GetAllPostsAsync();
        return Results.Ok(users);
      });
      app.MapGet("/posts/{id}",async (int id, DatabaseService db ) =>
      {
        try
        {
            var post = await db.GetPostByIdAsync(id);
            if (post == null)
                return Results.NotFound();
            
            return Results.Ok(post);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while getting the post: {ex.Message}");
        }
    });

      app.MapPost("/posts", async (Post post, DatabaseService db) =>
        {
          try
          {
            var newPost = await db.CreatePostAsync(post);
            return Results.Created($"/posts/{post.Id}", newPost);
          }
          catch (Exception ex)
          {
            return Results.Problem($"An error occurred while creating the post: {ex.Message}");
          }
        });
 app.MapDelete("/posts/{id}", async (int id, DatabaseService db) =>
      {
        try
        {
          bool deleted = await db.DeletePostAsync(id);

          if (deleted)
          {
            // Return a 204 No Content response
            return Results.NoContent();
          }
          else
          {
            // Return a 404 Not Found response
            return Results.NotFound();
          }
        }
        catch (Exception ex)
        {
          return Results.Problem($"An error occurred while deleting the post: {ex.Message}");
        }
      });

      app.MapPut("/posts/{id}", async (int id, Post updatedPost, DatabaseService db) =>
      {
        try
        {

          // Set the ID from the route parameter
          updatedPost.Id = id;

          // Update the Category
          var result = await db.UpdatePostAsync(updatedPost);
          if (result == null)
            return Results.NotFound();

          // Return the updated Category
          return Results.Ok(result);
        }
        catch (Exception ex)
        {
          return Results.Problem($"An error occurred while updating the Category: {ex.Message}");
        }
      });


    }
  }
}