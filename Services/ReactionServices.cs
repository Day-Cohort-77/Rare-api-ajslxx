using Npgsql;
using RareAPI.Models;

namespace RareAPI.Services
{
    public class ReactionServices
    {
        private readonly DatabaseService _databaseService;

        public ReactionServices(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private NpgsqlConnection CreateConnection()
        {
            return _databaseService.CreateConnection();
        }

        // Get all available reactions
        public async Task<List<Reaction>> GetAllReactionsAsync()
        {
            var reactions = new List<Reaction>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT id, label, image_url 
                FROM ""Reactions"" 
                ORDER BY id", connection);
            
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reactions.Add(new Reaction
                {
                    Id = reader.GetInt32(0),
                    Label = reader.GetString(1),
                    ImageUrl = reader.GetString(2)
                });
            }

            return reactions;
        }

        // Create a new reaction
        public async Task<Reaction?> CreateReactionAsync(string label, string imageUrl)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                INSERT INTO ""Reactions"" (label, image_url)
                VALUES (@label, @imageUrl)
                RETURNING id, label, image_url", connection);
            
            command.Parameters.AddWithValue("@label", label);
            command.Parameters.AddWithValue("@imageUrl", imageUrl);
            
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Reaction
                {
                    Id = reader.GetInt32(0),
                    Label = reader.GetString(1),
                    ImageUrl = reader.GetString(2)
                };
            }

            return null;
        }

        // Get a specific reaction by ID
        public async Task<Reaction?> GetReactionByIdAsync(int reactionId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT id, label, image_url 
                FROM ""Reactions"" 
                WHERE id = @reactionId", connection);
            
            command.Parameters.AddWithValue("@reactionId", reactionId);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Reaction
                {
                    Id = reader.GetInt32(0),
                    Label = reader.GetString(1),
                    ImageUrl = reader.GetString(2)
                };
            }

            return null;
        }

        // Add a reaction to a post
        public async Task<PostReaction?> AddPostReactionAsync(int userId, int reactionId, int postId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Check if user already reacted with this reaction to this post
            using var checkCommand = new NpgsqlCommand(@"
                SELECT id FROM ""PostReactions"" 
                WHERE user_id = @userId AND reaction_id = @reactionId AND post_id = @postId", connection);
            
            checkCommand.Parameters.AddWithValue("@userId", userId);
            checkCommand.Parameters.AddWithValue("@reactionId", reactionId);
            checkCommand.Parameters.AddWithValue("@postId", postId);
            
            var existingReaction = await checkCommand.ExecuteScalarAsync();
            if (existingReaction != null)
            {
                // User already reacted with this reaction, return existing
                return new PostReaction
                {
                    Id = (int)existingReaction,
                    UserId = userId,
                    ReactionId = reactionId,
                    PostId = postId
                };
            }

            // Add new reaction
            using var insertCommand = new NpgsqlCommand(@"
                INSERT INTO ""PostReactions"" (user_id, reaction_id, post_id)
                VALUES (@userId, @reactionId, @postId)
                RETURNING id", connection);
            
            insertCommand.Parameters.AddWithValue("@userId", userId);
            insertCommand.Parameters.AddWithValue("@reactionId", reactionId);
            insertCommand.Parameters.AddWithValue("@postId", postId);
            
            var newId = await insertCommand.ExecuteScalarAsync();
            if (newId != null)
            {
                return new PostReaction
                {
                    Id = (int)newId,
                    UserId = userId,
                    ReactionId = reactionId,
                    PostId = postId
                };
            }

            return null;
        }

        // Remove a reaction from a post
        public async Task<bool> RemovePostReactionAsync(int userId, int reactionId, int postId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                DELETE FROM ""PostReactions"" 
                WHERE user_id = @userId AND reaction_id = @reactionId AND post_id = @postId", connection);
            
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@reactionId", reactionId);
            command.Parameters.AddWithValue("@postId", postId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Get reaction counts for a post
        public async Task<List<ReactionCount>> GetPostReactionCountsAsync(int postId)
        {
            var reactionCounts = new List<ReactionCount>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT 
                    r.id as reaction_id,
                    r.label,
                    r.image_url,
                    COUNT(pr.id) as reaction_count
                FROM ""Reactions"" r
                LEFT JOIN ""PostReactions"" pr ON r.id = pr.reaction_id AND pr.post_id = @postId
                GROUP BY r.id, r.label, r.image_url
                ORDER BY r.id", connection);
            
            command.Parameters.AddWithValue("@postId", postId);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reactionCounts.Add(new ReactionCount
                {
                    ReactionId = reader.GetInt32(0),
                    Label = reader.GetString(1),
                    ImageUrl = reader.GetString(2),
                    Count = reader.GetInt32(3)
                });
            }

            return reactionCounts;
        }

        // Get user's reactions for a post
        public async Task<List<int>> GetUserPostReactionsAsync(int userId, int postId)
        {
            var userReactions = new List<int>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT reaction_id 
                FROM ""PostReactions"" 
                WHERE user_id = @userId AND post_id = @postId", connection);
            
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@postId", postId);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                userReactions.Add(reader.GetInt32(0));
            }

            return userReactions;
        }

        // Get post with reaction data
        public async Task<PostWithReactions?> GetPostWithReactionsAsync(int postId, int? userId = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Get post details
            using var postCommand = new NpgsqlCommand(@"
                SELECT id, user_id, category_id, title, publication_date, image_url, content, approved 
                FROM ""Posts"" 
                WHERE id = @postId", connection);
            
            postCommand.Parameters.AddWithValue("@postId", postId);
            using var postReader = await postCommand.ExecuteReaderAsync();
            
            if (!await postReader.ReadAsync())
                return null;

            var post = new Post
            {
                Id = postReader.GetInt32(0),
                UserId = postReader.GetInt32(1),
                CategoryId = postReader.GetInt32(2),
                Title = postReader.GetString(3),
                PublicationDate = postReader.GetDateTime(4),
                ImageUrl = postReader.IsDBNull(5) ? string.Empty : postReader.GetString(5),
                Content = postReader.GetString(6),
                Approved = postReader.GetBoolean(7)
            };

            await postReader.CloseAsync();

            // Get reaction counts
            var reactionCounts = await GetPostReactionCountsAsync(postId);

            // Get user reactions if userId provided
            var userReactions = new List<int>();
            if (userId.HasValue)
            {
                userReactions = await GetUserPostReactionsAsync(userId.Value, postId);
            }

            return new PostWithReactions
            {
                Post = post,
                ReactionCounts = reactionCounts,
                UserReactions = userReactions
            };
        }
    }
}
