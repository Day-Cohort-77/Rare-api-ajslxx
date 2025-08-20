
using Npgsql;
using RareAPI.Models;

namespace RareAPI.Services
{
    public class CommentServices
    {
        private readonly DatabaseService _databaseService;

        public CommentServices(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private NpgsqlConnection CreateConnection()
        {
            return _databaseService.CreateConnection();
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            string sql = @"
                INSERT INTO ""Comments"" (post_id, author_id, content) 
                VALUES (@postId, @authorId, @content)
                RETURNING id, post_id, author_id, content;";

            var parameters = new Dictionary<string, object>
            {
                { "@postId", comment.PostId },
                { "@authorId", comment.AuthorId },
                { "@content", comment.Content }
            };

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Comment
                {
                    Id = reader.GetInt32(0),           // id
                    PostId = reader.GetInt32(1),       // post_id
                    AuthorId = reader.GetInt32(2),     // author_id
                    Content = reader.GetString(3)      // content
                };
            }

            throw new InvalidOperationException("Failed to create comment");
        }

        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            string sql = @"
                SELECT 
                c.id AS Id,
                c.post_id AS PostId,
                c.author_id AS AuthorId,
                c.content AS Content,
                u.first_name AS AuthorFirstName,
                u.last_name AS AuthorLastName,
                u.username AS AuthorUsername
                FROM ""Comments"" c
                JOIN ""Users"" u ON c.author_id = u.id
                WHERE c.post_id = @postId
                ORDER BY c.id ASC;";

            var parameters = new Dictionary<string, object>
            {
                { "@postId", postId }
            };

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }

            using var reader = await command.ExecuteReaderAsync();

            var comments = new List<Comment>();
            while (await reader.ReadAsync())
            {
                comments.Add(new Comment
                {
                    Id = reader.GetInt32(0),        // c.id AS Id
                    PostId = reader.GetInt32(1),    // c.post_id AS PostId
                    AuthorId = reader.GetInt32(2),  // c.author_id AS AuthorId
                    Content = reader.GetString(3)   // c.content AS Content
                });
            }
            return comments;
        }

        // Get all comments
        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            string sql = @"
                SELECT 
                id AS Id,
                post_id AS PostId,
                author_id AS AuthorId,
                content AS Content
                FROM ""Comments""
                ORDER BY id ASC;";

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            var comments = new List<Comment>();
            while (await reader.ReadAsync())
            {
                comments.Add(new Comment
                {
                    Id = reader.GetInt32(0),        // id AS Id
                    PostId = reader.GetInt32(1),    // post_id AS PostId
                    AuthorId = reader.GetInt32(2),  // author_id AS AuthorId
                    Content = reader.GetString(3)   // content AS Content
                });
            }
            return comments;
        }

        // Update a comment
        public async Task<Comment?> UpdateCommentAsync(int commentId, string newContent)
        {
            string sql = @"
                UPDATE ""Comments"" 
                SET content = @content 
                WHERE id = @id
                RETURNING id, post_id, author_id, content;";

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@content", newContent);
            command.Parameters.AddWithValue("@id", commentId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Comment
                {
                    Id = reader.GetInt32(0),        // id
                    PostId = reader.GetInt32(1),    // post_id
                    AuthorId = reader.GetInt32(2),  // author_id
                    Content = reader.GetString(3)   // content
                };
            }

            return null; // Comment not found
        }

        // Delete a comment
        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            string sql = @"DELETE FROM ""Comments"" WHERE id = @id";

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", commentId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}