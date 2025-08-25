using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class CommentEndpoints
    {
        public static void MapCommentEndpoints(this WebApplication app)
        {
            // GET /comments
            app.MapGet("/comments", async (CommentServices commentService) =>
            {
                try
                {
                    var comments = await commentService.GetAllCommentsAsync();
                    return Results.Ok(comments);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving comments: {ex.Message}");
                }
            });

            // GET /comments/{id}
            app.MapGet("/comments/{id}", async (int id, CommentServices commentService) =>
            {
                try
                {
                    var comment = await commentService.GetCommentByIdAsync(id);
                    return comment != null ? Results.Ok(comment) : Results.NotFound();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving comment: {ex.Message}");
                }
            });

            // POST /comments
            app.MapPost("/comments", async (Comment comment, CommentServices commentService) =>
            {
                try
                {
                    var createdComment = await commentService.CreateCommentAsync(comment);
                    return createdComment != null ? Results.Created($"/comments/{createdComment.Id}", createdComment) : Results.BadRequest();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error creating comment: {ex.Message}");
                }
            });

            // PUT /comments/{id}
            app.MapPut("/comments/{id}", async (int id, UpdateCommentRequest request, CommentServices commentService) =>
            {
                try
                {
                    var updatedComment = await commentService.UpdateCommentAsync(id, request.Content);
                    return updatedComment != null ? Results.Ok(updatedComment) : Results.NotFound();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error updating comment: {ex.Message}");
                }
            });

            // DELETE /comments/{id}
            app.MapDelete("/comments/{id}", async (int id, CommentServices commentService) =>
            {
                try
                {
                    var deleted = await commentService.DeleteCommentAsync(id);
                    return deleted ? Results.NoContent() : Results.NotFound();
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error deleting comment: {ex.Message}");
                }
            });

            // GET /posts/{postId}/comments - Basic comments for a post
            app.MapGet("/posts/{postId}/comments", async (int postId, CommentServices commentService) =>
            {
                try
                {
                    var comments = await commentService.GetCommentsByPostIdAsync(postId);
                    return Results.Ok(comments);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving comments for post: {ex.Message}");
                }
            });

            // GET /posts/{postId}/comments-with-details - Comments with author and post details
            app.MapGet("/posts/{postId}/comments-with-details", async (int postId, CommentServices commentService) =>
            {
                try
                {
                    var (post, comments) = await commentService.GetCommentsWithDetailsByPostIdAsync(postId);
                    
                    if (post == null)
                        return Results.NotFound($"Post with ID {postId} not found");
                        
                    return Results.Ok(new { 
                        post = new { 
                            id = post.Id, 
                            title = post.Title 
                        },
                        comments = comments.Select(c => new {
                            id = c.Id,
                            subject = c.Subject,
                            content = c.Content,
                            authorDisplayName = c.AuthorDisplayName,
                            createdOn = c.CreatedOn.ToString("MM/dd/yyyy")
                        })
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving comments with details: {ex.Message}");
                }
            });
        }
    }
}
