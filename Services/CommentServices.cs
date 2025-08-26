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

        public async Task<Comment?> CreateCommentAsync(Comment comment)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO ""Comments"" (post_id, author_id, subject, content, created_on)
                VALUES (@postId, @authorId, @subject, @content, @createdOn)
                RETURNING id, post_id, author_id, subject, content, created_on";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@postId", comment.PostId);
            command.Parameters.AddWithValue("@authorId", comment.AuthorId);
            command.Parameters.AddWithValue("@subject", comment.Subject ?? string.Empty);
            command.Parameters.AddWithValue("@content", comment.Content);
            command.Parameters.AddWithValue("@createdOn", DateTime.UtcNow);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Comment
                {
                    Id = reader.GetInt32(0),
                    PostId = reader.GetInt32(1),
                    AuthorId = reader.GetInt32(2),
                    Subject = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedOn = reader.GetDateTime(5)
                };
            }

            return null;
        }

        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = new List<Comment>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT id, post_id, author_id, subject, content, created_on 
                FROM ""Comments"" 
                WHERE post_id = @postId 
                ORDER BY created_on DESC", connection);
            
            command.Parameters.AddWithValue("@postId", postId);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                comments.Add(new Comment
                {
                    Id = reader.GetInt32(0),
                    PostId = reader.GetInt32(1),
                    AuthorId = reader.GetInt32(2),
                    Subject = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedOn = reader.GetDateTime(5)
                });
            }

            return comments;
        }

        public async Task<(Post? post, List<Comment> comments)> GetCommentsWithDetailsByPostIdAsync(int postId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // First get the post details
            var postSql = @"SELECT id, user_id, category_id, title, publication_date, image_url, content, approved 
                           FROM ""Posts"" WHERE id = @postId";
            
            Post? post = null;
            using (var postCommand = new NpgsqlCommand(postSql, connection))
            {
                postCommand.Parameters.AddWithValue("@postId", postId);
                using var postReader = await postCommand.ExecuteReaderAsync();
                
                if (await postReader.ReadAsync())
                {
                    post = new Post
                    {
                        Id = postReader.GetInt32(0),
                        UserId = postReader.GetInt32(1),
                        CategoryId = postReader.GetInt32(2),
                        Title = postReader.GetString(3),
                        PublicationDate = postReader.GetDateTime(4),
                        ImageUrl = postReader.IsDBNull(5) ? null : postReader.GetString(5),
                        Content = postReader.GetString(6),
                        Approved = postReader.GetBoolean(7)
                    };
                }
            }

            // Then get comments with author details
            var commentsSql = @"
                SELECT 
                    c.id,
                    c.post_id,
                    c.author_id,
                    c.subject,
                    c.content,
                    c.created_on,
                    CONCAT(u.first_name, ' ', u.last_name) as author_display_name,
                    p.title as post_title
                FROM ""Comments"" c
                JOIN ""Users"" u ON c.author_id = u.id  
                JOIN ""Posts"" p ON c.post_id = p.id
                WHERE c.post_id = @postId
                ORDER BY c.created_on DESC";

            var comments = new List<Comment>();
            using var commentsCommand = new NpgsqlCommand(commentsSql, connection);
            commentsCommand.Parameters.AddWithValue("@postId", postId);
            using var commentsReader = await commentsCommand.ExecuteReaderAsync();

            while (await commentsReader.ReadAsync())
            {
                comments.Add(new Comment
                {
                    Id = commentsReader.GetInt32(0),
                    PostId = commentsReader.GetInt32(1),
                    AuthorId = commentsReader.GetInt32(2),
                    Subject = commentsReader.IsDBNull(3) ? string.Empty : commentsReader.GetString(3),
                    Content = commentsReader.GetString(4),
                    CreatedOn = commentsReader.GetDateTime(5),
                    AuthorDisplayName = commentsReader.GetString(6),
                    PostTitle = commentsReader.GetString(7)
                });
            }

            return (post, comments);
        }

        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            var comments = new List<Comment>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT id, post_id, author_id, subject, content, created_on 
                FROM ""Comments"" 
                ORDER BY created_on DESC", connection);
            
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                comments.Add(new Comment
                {
                    Id = reader.GetInt32(0),
                    PostId = reader.GetInt32(1),
                    AuthorId = reader.GetInt32(2),
                    Subject = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedOn = reader.GetDateTime(5)
                });
            }

            return comments;
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT id, post_id, author_id, subject, content, created_on 
                FROM ""Comments"" 
                WHERE id = @commentId", connection);
            
            command.Parameters.AddWithValue("@commentId", commentId);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Comment
                {
                    Id = reader.GetInt32(0),
                    PostId = reader.GetInt32(1),
                    AuthorId = reader.GetInt32(2),
                    Subject = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedOn = reader.GetDateTime(5)
                };
            }

            return null;
        }

        public async Task<Comment?> GetCommentWithDetailsByIdAsync(int commentId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT 
                    c.id,
                    c.post_id,
                    c.author_id,
                    c.subject,
                    c.content,
                    c.created_on,
                    CONCAT(u.first_name, ' ', u.last_name) as author_display_name,
                    p.title as post_title
                FROM ""Comments"" c
                JOIN ""Users"" u ON c.author_id = u.id  
                JOIN ""Posts"" p ON c.post_id = p.id
                WHERE c.id = @commentId", connection);
            
            command.Parameters.AddWithValue("@commentId", commentId);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Comment
                {
                    Id = reader.GetInt32(0),
                    PostId = reader.GetInt32(1),
                    AuthorId = reader.GetInt32(2),
                    Subject = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedOn = reader.GetDateTime(5),
                    AuthorDisplayName = reader.GetString(6),
                    PostTitle = reader.GetString(7)
                };
            }

            return null;
        }

        public async Task<Comment?> UpdateCommentAsync(int commentId, string newSubject, string newContent)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                UPDATE ""Comments"" 
                SET subject = @subject, content = @content 
                WHERE id = @commentId 
                RETURNING id, post_id, author_id, subject, content, created_on", connection);
            
            command.Parameters.AddWithValue("@subject", newSubject ?? string.Empty);
            command.Parameters.AddWithValue("@content", newContent);
            command.Parameters.AddWithValue("@commentId", commentId);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Comment
                {
                    Id = reader.GetInt32(0),
                    PostId = reader.GetInt32(1),
                    AuthorId = reader.GetInt32(2),
                    Subject = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedOn = reader.GetDateTime(5)
                };
            }

            return null;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                DELETE FROM ""Comments"" 
                WHERE id = @commentId", connection);
            
            command.Parameters.AddWithValue("@commentId", commentId);
            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
}