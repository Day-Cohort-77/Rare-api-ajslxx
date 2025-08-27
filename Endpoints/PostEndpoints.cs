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
                    return newPost is not null ? Results.Created($"/posts/{newPost.Id}", newPost) : Results.BadRequest("Failed to create post");
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
                    var result = await postService.UpdatePostAsync(id, updatedPost);
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
                        
                        return Results.NoContent();
                    }
                    else
                    {
                        
                        return Results.NotFound();
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred while deleting the post: {ex.Message}");
                }
            });

            // Post Header Image Endpoints
            app.MapPost("/posts/{postId}/header-image", async (int postId, UpdatePostHeaderImageRequest request, PostServices postService) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.ImageData) || string.IsNullOrWhiteSpace(request.FileName))
                    {
                        return Results.BadRequest("ImageData and FileName are required");
                    }

                    var result = await postService.UpdatePostHeaderImageAsync(postId, request);
                    
                    if (result.Success)
                    {
                        return Results.Ok(result);
                    }
                    else
                    {
                        return Results.BadRequest(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error updating post header image: {ex.Message}");
                }
            });

            app.MapGet("/posts/{postId}/header-image", async (int postId, PostServices postService) =>
            {
                try
                {
                    var headerImageUrl = await postService.GetPostHeaderImageAsync(postId);
                    
                    if (string.IsNullOrEmpty(headerImageUrl))
                    {
                        return Results.NotFound("Post header image not found");
                    }

                    return Results.Ok(new { postId, imageUrl = headerImageUrl });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving post header image: {ex.Message}");
                }
            });

            app.MapDelete("/posts/{postId}/header-image", async (int postId, PostServices postService) =>
            {
                try
                {
                    var success = await postService.DeletePostHeaderImageAsync(postId);
                    return success ? Results.NoContent() : Results.NotFound("Post not found");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error deleting post header image: {ex.Message}");
                }
            });
        }
    }
}
