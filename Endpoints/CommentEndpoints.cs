using RareAPI.Services;
using RareAPI.Models;

namespace RareAPI.Endpoints
{
    public static class CommentEndpoints
    {
        public static void MapCommentEndpoints(this WebApplication app)
        {
            
            app.MapGet("/comments", async (CommentServices commentService) =>
            {
                return await commentService.GetAllCommentsAsync();
            });

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

            
            app.MapGet("/posts/{postId}/comments", async (int postId, CommentServices commentService) =>
            {
                return await commentService.GetCommentsByPostIdAsync(postId);
            });

            
            app.MapPost("/comments", async (Comment comment, CommentServices commentService) =>
            {
                var createdComment = await commentService.CreateCommentAsync(comment);
                return Results.Created($"/comments/{createdComment.Id}", createdComment);
            });

            
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

            
            app.MapDelete("/comments/{id}", async (int id, CommentServices commentService) =>
            {
                var deleted = await commentService.DeleteCommentAsync(id);
                if (deleted)
                {
                    return Results.NoContent(); 
                }
                else
                {
                    return Results.NotFound(); 
                }
            });
        }
    }
}
