using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class PostEndpoints
    {
        public static void MapPostEndpoints(this WebApplication app)
        {
            app.MapGet("/posts", async (PostServices postService) =>
            {
                var posts = await postService.GetAllPostsAsync();
                return Results.Ok(posts);
            });

            app.MapPost("/posts", async (Post post, PostServices postService) =>
            {
                try
                {
                    var newPost = await postService.CreatePostAsync(post);
                    return Results.Created($"/posts/{newPost.Id}", newPost);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred while creating the post: {ex.Message}");
                }
            });

            app.MapGet("/posts/{id}", async (int id, PostServices postService) =>
            {
                var post = await postService.GetPostByIdAsync(id);
                return post is not null ? Results.Ok(post) : Results.NotFound();
            });

            app.MapPut("/posts/{id}", async (int id, Post updatedPost, PostServices postService) =>
            {
                try
                {
                    // Set the ID from the route parameter
                    updatedPost.Id = id;

                    // Update the Post
                    var result = await postService.UpdatePostAsync(updatedPost);

                    // Return the updated Post or NotFound if it doesn't exist
                    return result is not null ? Results.Ok(result) : Results.NotFound();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred while updating the post: {ex.Message}");
                }
            });

            app.MapDelete("/posts/{id}", async (int id, PostServices postService) =>
            {
                try
                {
                    bool deleted = await postService.DeletePostAsync(id);

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
        }
    }
}
