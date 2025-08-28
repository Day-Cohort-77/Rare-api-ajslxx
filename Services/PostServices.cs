
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

        public async Task<List<Post>> GetPostsByTagIdAsync(int tagId)
        {
            var posts = new List<Post>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"
        SELECT DISTINCT
            p.id, 
            p.user_id, 
            p.category_id, 
            p.title, 
            p.publication_date, 
            p.image_url, 
            p.content, 
            p.approved 
        FROM ""Posts"" p
        INNER JOIN ""PostTags"" pt ON p.id = pt.post_id
        WHERE pt.tag_id = @tagId AND p.approved = true
        ORDER BY p.publication_date DESC";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@tagId", tagId);

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

        public async Task<List<Post>> GetPostsByAuthorIdAsync(int id)
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
                WHERE user_id = @id
                ORDER BY publication_date DESC", connection);

            command.Parameters.AddWithValue("@id", id);
            
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

        // Post Header Image Methods
        public async Task<PostHeaderImageResponse> UpdatePostHeaderImageAsync(int postId, UpdatePostHeaderImageRequest request)
        {
            var response = new PostHeaderImageResponse
            {
                PostId = postId,
                UpdatedOn = DateTime.UtcNow
            };

            try
            {
                // Validate base64 image data
                if (string.IsNullOrWhiteSpace(request.ImageData))
                {
                    response.Success = false;
                    response.Message = "Image data is required";
                    return response;
                }

                // Validate and process base64 data
                var base64Data = request.ImageData;

                // Remove data URL prefix if present (e.g., "data:image/jpeg;base64,")
                if (base64Data.StartsWith("data:"))
                {
                    var commaIndex = base64Data.IndexOf(',');
                    if (commaIndex >= 0)
                    {
                        base64Data = base64Data.Substring(commaIndex + 1);
                    }
                }

                // Validate base64 format
                try
                {
                    var imageBytes = Convert.FromBase64String(base64Data);

                    // Basic size validation (limit to 10MB for header images)
                    if (imageBytes.Length > 10 * 1024 * 1024)
                    {
                        response.Success = false;
                        response.Message = "Header image size cannot exceed 10MB";
                        return response;
                    }

                    // Basic image format validation by checking file headers
                    if (!IsValidImageFormat(imageBytes))
                    {
                        response.Success = false;
                        response.Message = "Invalid image format. Only JPEG, PNG, GIF, and WebP are supported";
                        return response;
                    }
                }
                catch (FormatException)
                {
                    response.Success = false;
                    response.Message = "Invalid base64 image data";
                    return response;
                }

                // Create data URL for storage
                var contentType = request.ContentType ?? GetContentTypeFromFileName(request.FileName);
                var dataUrl = $"data:{contentType};base64,{base64Data}";

                // Update post's image URL in database
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var updateSql = @"
                    UPDATE ""Posts""
                    SET image_url = @imageUrl
                    WHERE id = @postId
                    RETURNING image_url";

                using var command = new NpgsqlCommand(updateSql, connection);
                command.Parameters.AddWithValue("@postId", postId);
                command.Parameters.AddWithValue("@imageUrl", dataUrl);

                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    response.Success = true;
                    response.ImageUrl = result.ToString()!;
                    response.FileName = request.FileName;
                    response.Message = "Post header image updated successfully";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Post not found";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating post header image: {ex.Message}";
            }

            return response;
        }

        public async Task<string?> GetPostHeaderImageAsync(int postId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT image_url FROM \"Posts\" WHERE id = @postId";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@postId", postId);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<bool> DeletePostHeaderImageAsync(int postId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var updateSql = @"
                UPDATE ""Posts""
                SET image_url = ''
                WHERE id = @postId";

            using var command = new NpgsqlCommand(updateSql, connection);
            command.Parameters.AddWithValue("@postId", postId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Helper methods for image validation (same as UserServices)
        private static bool IsValidImageFormat(byte[] imageBytes)
        {
            if (imageBytes.Length < 4) return false;

            // Check for common image file signatures
            // JPEG
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
                return true;

            // PNG
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return true;

            // GIF
            if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46)
                return true;

            // WebP
            if (imageBytes.Length >= 12 &&
                imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 && imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
                return true;

            return false;
        }

        private static string GetContentTypeFromFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg" // default
            };
        }
    }
}
