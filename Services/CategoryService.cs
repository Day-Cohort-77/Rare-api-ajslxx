using Npgsql;
using RareAPI.Models;
using System.Data;
using RareAPI.Services;

public class CategoryService : DatabaseService
{
  public CategoryService(IConfiguration config) : base(config) { }

          public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var response = new List<Category>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
              @"SELECT 
                id AS category_id, 
                label AS category_label
                FROM ""Categories"";",
               connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.Add(new Category
                {
                    Id = reader.GetInt32("category_id"),
                    Label = reader.GetString("category_label"),
                });
            }

            return response;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"INSERT INTO ""Categories"" (""label"")
                    VALUES (@label)
                    RETURNING id;",
                connection);

            command.Parameters.AddWithValue("@label", category.Label);

            // Execute the command and get the generated ID
            category.Id = Convert.ToInt32(await command.ExecuteScalarAsync());

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"DELETE FROM ""Categories"" WHERE ""id"" = @id;",
                connection);

            command.Parameters.AddWithValue("@id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT ""id"", ""label"" FROM ""Categories"" WHERE ""id"" = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Category
                {
                    Id = reader.GetInt32(0),
                    Label = reader.GetString(1)
                };
            }

            return null;
        }

        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"UPDATE ""Categories""
                SET ""label"" = @label
                WHERE ""id"" = @id",
                connection);

            command.Parameters.AddWithValue("@id", category.Id);
            command.Parameters.AddWithValue("@label", category.Label);

            // Execute the command
            await command.ExecuteNonQueryAsync();

            // Retrieve and return the updated category
            return await GetCategoryByIdAsync(category.Id);
        }
}
