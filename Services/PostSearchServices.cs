using Npgsql;
using RareAPI.Models;

namespace RareAPI.Services
{
    public class PostSearchService
    {
        private readonly DatabaseService _databaseService;

        public PostSearchService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private NpgsqlConnection CreateConnection()
        {
            return _databaseService.CreateConnection();
        }

        public async Task<List<Post>> SearchAsync(string query)
        {
            var results = new List<Post>();
            using var connection = CreateConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT id, user_id, category_id, title, publication_date, image_url, content, approved
                FROM ""Posts""
                WHERE title ILIKE @query
                ORDER BY publication_date DESC
            ";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@query", $"%{query}%");

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new Post
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

            return results;
        }
    }
}