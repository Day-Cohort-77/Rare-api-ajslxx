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

            app.MapGet("/posts/author/{id}", async (int id, PostServices postService) =>
            {
                var posts = await postService.GetPostsByAuthorIdAsync(id);
                return Results.Ok(posts);
            });

            app.MapGet("/posts/author/format/{id}", async (int id, PostServices postService, CategoriesServices categoriesService) =>
            {
                var formattedPostList = new List<FormattedPost>();

                var posts = await postService.GetPostsByAuthorIdAsync(id);
                
                foreach (var post in posts)
                {
                    var category = await categoriesService.GetCategoryByIdAsync(post.CategoryId);
                    formattedPostList.Add(new FormattedPost
                    {
                        Title = post.Title,
                        Content = post.Content,
                        ImageUrl = post.ImageUrl,
                        Category = category.Label
                    }); 
                }
                
                return Results.Ok(formattedPostList);
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
        }
    }
}
