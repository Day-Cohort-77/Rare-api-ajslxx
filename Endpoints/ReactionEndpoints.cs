using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class ReactionEndpoints
    {
        public static void MapReactionEndpoints(this WebApplication app)
        {
            // GET /reactions - Get all available reactions
            app.MapGet("/reactions", async (ReactionServices reactionService) =>
            {
                try
                {
                    var reactions = await reactionService.GetAllReactionsAsync();
                    return Results.Ok(reactions);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving reactions: {ex.Message}");
                }
            });

            // POST /posts/{postId}/reactions - Add a reaction to a post
            app.MapPost("/posts/{postId}/reactions", async (int postId, AddPostReactionRequest request, ReactionServices reactionService) =>
            {
                try
                {
                    var postReaction = await reactionService.AddPostReactionAsync(request.UserId, request.ReactionId, postId);
                    return postReaction != null 
                        ? Results.Created($"/posts/{postId}/reactions", postReaction) 
                        : Results.BadRequest("Failed to add reaction");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error adding reaction: {ex.Message}");
                }
            });

            // DELETE /posts/{postId}/reactions - Remove a reaction from a post
            app.MapDelete("/posts/{postId}/reactions", async (int postId, int userId, int reactionId, ReactionServices reactionService) =>
            {
                try
                {
                    var removed = await reactionService.RemovePostReactionAsync(userId, reactionId, postId);
                    return removed ? Results.NoContent() : Results.NotFound("Reaction not found");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error removing reaction: {ex.Message}");
                }
            });

            // GET /posts/{postId}/reactions - Get reaction counts for a post
            app.MapGet("/posts/{postId}/reactions", async (int postId, ReactionServices reactionService) =>
            {
                try
                {
                    var reactionCounts = await reactionService.GetPostReactionCountsAsync(postId);
                    return Results.Ok(reactionCounts);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving reaction counts: {ex.Message}");
                }
            });

            // GET /posts/{postId}/reactions/user/{userId} - Get user's reactions for a post
            app.MapGet("/posts/{postId}/reactions/user/{userId}", async (int postId, int userId, ReactionServices reactionService) =>
            {
                try
                {
                    var userReactions = await reactionService.GetUserPostReactionsAsync(userId, postId);
                    return Results.Ok(userReactions);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving user reactions: {ex.Message}");
                }
            });

            // GET /posts/{postId}/with-reactions - Get post with all reaction data
            app.MapGet("/posts/{postId}/with-reactions", async (int postId, int? userId, ReactionServices reactionService) =>
            {
                try
                {
                    var postWithReactions = await reactionService.GetPostWithReactionsAsync(postId, userId);
                    return postWithReactions != null 
                        ? Results.Ok(postWithReactions) 
                        : Results.NotFound($"Post with ID {postId} not found");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving post with reactions: {ex.Message}");
                }
            });
        }
    }
}
