
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
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT 
                id AS Id,
                user_id AS UserId,
                category_id AS CategoryId,
                title AS Title,
                publication_date AS PublicationDate,
                image_url AS ImageUrl,
                content AS Content,
                approved AS Approved
                FROM ""Posts"";",
                connection);

            using var reader = await command.ExecuteReaderAsync();

            var posts = new List<Post>();
            while (await reader.ReadAsync())
            {
                posts.Add(new Post
                {
                    Id = reader.GetInt32("Id"),
                    UserId = reader.GetInt32("UserId"),
                    CategoryId = reader.GetInt32("CategoryId"),
                    Title = reader.GetString("Title"),
                    PublicationDate = reader.GetDateTime("PublicationDate"),
                    ImageUrl = reader.GetString("ImageUrl"),
                    Content = reader.GetString("Content"),
                    Approved = reader.GetBoolean("Approved")
                });
            }
            return posts;
        }
        public async Task<Post> CreatePostAsync(Post post)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"INSERT INTO ""Posts"" (""user_id"", ""category_id"", ""title"", ""publication_date"", ""image_url"", ""content"", ""approved"")
                    VALUES (@user_id, @category_id, @title, @publication_date, @image_url, @content, @approved)
                    RETURNING id;",
                connection);

            command.Parameters.AddWithValue("@user_id", post.UserId);
            command.Parameters.AddWithValue("@category_id", post.CategoryId);
            command.Parameters.AddWithValue("@title", post.Title);
            command.Parameters.AddWithValue("@publication_date", post.PublicationDate);
            command.Parameters.AddWithValue("@image_url", post.ImageUrl);
            command.Parameters.AddWithValue("@content", post.Content);
            command.Parameters.AddWithValue("@approved", post.Approved);

            // Execute the command and get the generated ID
            post.Id = Convert.ToInt32(await command.ExecuteScalarAsync());

            return post;
        }

        public async Task<Post> UpdatePostAsync(Post post)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();


            using var command = new NpgsqlCommand(
          @"UPDATE ""Posts""
            SET ""user_id"" = @user_id,
            ""category_id"" = @category_id,
            ""title"" = @title,
            ""publication_date"" = @publication_date,
            ""image_url"" = @image_url,
            ""content"" = @content,
             ""approved"" = @approved
            WHERE ""id"" = @id",
 connection);

            command.Parameters.AddWithValue("@user_id", post.UserId);
            command.Parameters.AddWithValue("@category_id", post.CategoryId);
            command.Parameters.AddWithValue("@title", post.Title);
            command.Parameters.AddWithValue("@publication_date", post.PublicationDate);
            command.Parameters.AddWithValue("@image_url", post.ImageUrl);
            command.Parameters.AddWithValue("@content", post.Content);
            command.Parameters.AddWithValue("@approved", post.Approved);
            command.Parameters.AddWithValue("@id", post.Id);

            // Execute the command
            await command.ExecuteNonQueryAsync();

            // Retrieve and return the updated category
            return await GetPostByIdAsync(post.Id);
        }

     public async Task<Post?> GetPostByIdAsync(int postId)
        {
            string sql = @"
                SELECT
                id AS Id,
                user_id AS UserId,
                category_id AS CategoryId,
                title AS Title,
                publication_date AS PublicationDate,
                image_url AS ImageUrl,
                content AS Content,
                approved AS Approved

                FROM ""Post""
                WHERE id = @id;";
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", postId);
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

            using var command = new NpgsqlCommand(
                @"DELETE FROM ""Posts"" WHERE ""id"" = @id;",
                connection);

            command.Parameters.AddWithValue("@id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }




    }
}