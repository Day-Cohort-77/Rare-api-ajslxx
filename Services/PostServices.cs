
using Npgsql;
using RareAPI.Models;

namespace RareAPI.Services
{
    public class PostServices
    {
        private readonly DatabaseService _databaseService;

        public PostServices(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private NpgsqlConnection CreateConnection()
        {
            return _databaseService.CreateConnection();
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            var posts = new List<Post>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT 
                    id, 
                    user_id, 
                    category_id, 
                    title, 
                    publication_date, 
                    image_url, 
                    content, 
                    approved 
                FROM ""Posts"" 
                ORDER BY publication_date DESC", connection);
            
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                posts.Add(new Post
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    PublicationDate = reader.GetDateTime(4),
                    ImageUrl = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.GetBoolean(7)
                });
            }

            return posts;
        }

        public async Task<Post?> GetPostByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    id, 
                    user_id, 
                    category_id, 
                    title, 
                    publication_date, 
                    image_url, 
                    content, 
                    approved 
                FROM ""Posts"" 
                WHERE id = @id";
            
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    PublicationDate = reader.GetDateTime(4),
                    ImageUrl = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.GetBoolean(7)
                };
            }

            return null;
        }

        public async Task<Post?> CreatePostAsync(Post newPost)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var insertSql = @"
                INSERT INTO ""Posts"" (user_id, category_id, title, publication_date, image_url, content, approved)
                VALUES (@userId, @categoryId, @title, @publicationDate, @imageUrl, @content, @approved)
                RETURNING id, user_id, category_id, title, publication_date, image_url, content, approved";

            using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@userId", newPost.UserId);
            command.Parameters.AddWithValue("@categoryId", newPost.CategoryId);
            command.Parameters.AddWithValue("@title", newPost.Title);
            command.Parameters.AddWithValue("@publicationDate", newPost.PublicationDate);
            command.Parameters.AddWithValue("@imageUrl", newPost.ImageUrl);
            command.Parameters.AddWithValue("@content", newPost.Content);
            command.Parameters.AddWithValue("@approved", newPost.Approved);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    PublicationDate = reader.GetDateTime(4),
                    ImageUrl = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.GetBoolean(7)
                };
            }

            return null;
        }

        public async Task<Post?> UpdatePostAsync(int id, Post updatedPost)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var updateSql = @"
                UPDATE ""Posts""
                SET user_id = @userId, category_id = @categoryId, title = @title, 
                    publication_date = @publicationDate, image_url = @imageUrl, 
                    content = @content, approved = @approved
                WHERE id = @id
                RETURNING id, user_id, category_id, title, publication_date, image_url, content, approved";

            using var command = new NpgsqlCommand(updateSql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@userId", updatedPost.UserId);
            command.Parameters.AddWithValue("@categoryId", updatedPost.CategoryId);
            command.Parameters.AddWithValue("@title", updatedPost.Title);
            command.Parameters.AddWithValue("@publicationDate", updatedPost.PublicationDate);
            command.Parameters.AddWithValue("@imageUrl", updatedPost.ImageUrl);
            command.Parameters.AddWithValue("@content", updatedPost.Content);
            command.Parameters.AddWithValue("@approved", updatedPost.Approved);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    PublicationDate = reader.GetDateTime(4),
                    ImageUrl = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.GetBoolean(7)
                };
            }

            return null;
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var deleteSql = "DELETE FROM \"Posts\" WHERE id = @id";
            using var command = new NpgsqlCommand(deleteSql, connection);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(int userId)
        {
            var posts = new List<Post>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    id, 
                    user_id, 
                    category_id, 
                    title, 
                    publication_date, 
                    image_url, 
                    content, 
                    approved 
                FROM ""Posts"" 
                WHERE user_id = @userId 
                ORDER BY publication_date DESC";
            
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                posts.Add(new Post
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    PublicationDate = reader.GetDateTime(4),
                    ImageUrl = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.GetBoolean(7)
                });
            }

            return posts;
        }
    }
}
