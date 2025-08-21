using RareAPI.Services;
using RareAPI.Models;

namespace RareAPI.Endpoints
{
    public static class CommentEndpoints
    {
        public static void MapCommentEndpoints(this WebApplication app)
        {
            // GET all comments
            app.MapGet("/comments", async (CommentServices commentService) =>
            {
                return await commentService.GetAllCommentsAsync();
            });

            // GET a single comment by ID
            app.MapGet("/comments/{id}", async (int id, CommentServices commentService) =>
            {
                var comment = await commentService.GetCommentByIdAsync(id);
                if (comment != null)
                {
                    return Results.Ok(comment);
                }
                else
                {
                    return Results.NotFound();
                }
            });

            // GET comments for a specific post
            app.MapGet("/posts/{postId}/comments", async (int postId, CommentServices commentService) =>
            {
                return await commentService.GetCommentsByPostIdAsync(postId);
            });

            // POST create a new comment
            app.MapPost("/comments", async (Comment comment, CommentServices commentService) =>
            {
                var createdComment = await commentService.CreateCommentAsync(comment);
                return Results.Created($"/comments/{createdComment.Id}", createdComment);
            });

            // PUT update a comment
            app.MapPut("/comments/{id}", async (int id, UpdateCommentRequest request, CommentServices commentService) =>
            {
                var updatedComment = await commentService.UpdateCommentAsync(id, request.Content);
                if (updatedComment != null)
                {
                    return Results.Ok(updatedComment);
                }
                else
                {
                    return Results.NotFound();
                }
            });

            // DELETE a comment
            app.MapDelete("/comments/{id}", async (int id, CommentServices commentService) =>
            {
                var deleted = await commentService.DeleteCommentAsync(id);
                if (deleted)
                {
                    return Results.NoContent(); // 204 No Content - successful deletion
                }
                else
                {
                    return Results.NotFound(); // 404 Not Found - comment doesn't exist
                }
            });
        }
    }
}
