using Npgsql;
using RareAPI.Models;

namespace RareAPI.Services
{
    public class TagService
    {
        private readonly DatabaseService _databaseService;

        public TagService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            var response = new List<Tag>();

            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
              @"SELECT id, label FROM ""Tags"";",
               connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.Add(new Tag
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Label = reader.GetString(reader.GetOrdinal("label"))
                });
            }

            return response;
        }

        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            using var connection = _databaseService.CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT id, label FROM ""Tags"" WHERE id = @id;",
                connection);

            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Tag
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Label = reader.GetString(reader.GetOrdinal("label"))
                };
            }

            return null;
        }

        public async Task AddTagAsync(string label)
        {
            const string sql = @"INSERT INTO ""Tags"" (label) VALUES (@label);";

            var parameters = new Dictionary<string, object>
            {
                { "@label", label }
            };

            await _databaseService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task UpdateTagAsync(int id, string label)
        {
            const string sql = @"UPDATE ""Tags"" SET label = @label WHERE id = @id;";

            var parameters = new Dictionary<string, object>
            {
                { "@id", id },
                { "@label", label }
            };

            await _databaseService.ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task DeleteTagAsync(int id)
        {
            const string sql = @"DELETE FROM ""Tags"" WHERE id = @id;";

            var parameters = new Dictionary<string, object>
            {
                { "@id", id }
            };

            await _databaseService.ExecuteNonQueryAsync(sql, parameters);
        }
    }
}