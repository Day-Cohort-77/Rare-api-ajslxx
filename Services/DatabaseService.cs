using Npgsql;
using RareAPI.Models;
using System.Data;

namespace RareAPI.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RareAPIConnectionString") ??
                throw new InvalidOperationException("Connection string 'RareAPIConnectionString' not found.");
        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Helper method to execute non-query SQL commands
        public async Task ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            await command.ExecuteNonQueryAsync();
        }

        public async Task InitializeDatabaseAsync()
        {
            // Connect to postgres to check/create the database
            var masterConnStr = _connectionString.Replace("Database=RareAPI", "Database=postgres");
            using var masterConnection = new NpgsqlConnection(masterConnStr);
            await masterConnection.OpenAsync();

            // Check if database exists (PostgreSQL database names are case-sensitive)
            using var checkCommand = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = 'RareAPI'",
                masterConnection);
            var exists = await checkCommand.ExecuteScalarAsync();

            if (exists == null)
            {
                // Create the database
                using var createDbCommand = new NpgsqlCommand(
                    "CREATE DATABASE \"RareAPI\"",
                    masterConnection);
                await createDbCommand.ExecuteNonQueryAsync();
            }

            // Close the master connection before connecting to RareAPI
            await masterConnection.CloseAsync();

            // Now connect to the RareAPI database and check/create tables
            using var rareApiConnection = new NpgsqlConnection(_connectionString);
            await rareApiConnection.OpenAsync();

            // Check if tables already exist
            using var checkTablesCommand = new NpgsqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Users'",
                rareApiConnection);
            var tableExists = (long)(await checkTablesCommand.ExecuteScalarAsync() ?? 0L);

            if (tableExists == 0)
            {
                // Tables don't exist, create them
                string sql = File.ReadAllText("database-setup.sql");
                using var setupCommand = new NpgsqlCommand(sql, rareApiConnection);
                await setupCommand.ExecuteNonQueryAsync();
            }
        }
        public async Task SeedDatabaseAsync()
        {
            // Check if data already exists
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Check if user table has data
            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", connection);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (count > 0)
            {
                // Data already exists, no need to seed
                return;
            }

            await ExecuteNonQueryAsync(@"
                INSERT INTO ""Users"" (first_name, last_name, email, bio, username, password, profile_image_url, created_on, active) VALUES
                ('Billy', 'Bob', 'billy@bob.com', 'I am Billy Bob', 'BillyBob', 'mycoolpass', 'https://www.thedailybeast.com/resizer/7-n47tS_FIUHO6A0UWE2XxsDki0=/arc-photo-thedailybeast/arc2-prod/public/GBJAOT4VF5IM7BLNH2I6MWRKGU.png', (TO_DATE('08/12/2025', 'MM/DD/YYYY')), true),
                ('Jimmy', 'John', 'jimmy@john.com', 'I am Jimmy John', 'JimmyJohn', 'ExcellentPassword', 'https://hips.hearstapps.com/hmg-prod/images/screenshot-2024-10-28-at-4-38-05-pm-671ff63778f27.png?crop=0.494xw:1.00xh;0.306xw,0&resize=1200:*', (TO_DATE('07/01/2022', 'MM/DD/YYYY')), true);
            ");
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var response = new List<User>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
              @"SELECT 
                id AS user_id, 
                first_name AS user_first_name,
                last_name AS user_last_name, 
                email AS user_email, 
                bio AS user_bio, 
                username AS user_username, 
                password AS user_password, 
                profile_image_url AS user_profile_image_url,
                created_on AS user_created_on,
                active AS user_active
                FROM ""Users"";",
               connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.Add(new User
                {
                    Id = reader.GetInt32("user_id"),
                    FirstName = reader.GetString("user_first_name"),
                    LastName = reader.GetString("user_last_name"),
                    Email = reader.GetString("user_email"),
                    Bio = reader.GetString("user_bio"),
                    Username = reader.GetString("user_username"),
                    Password = reader.GetString("user_password"),
                    ProfileImageUrl = reader.GetString("user_profile_image_url"),
                    CreatedOn = reader.GetDateTime("user_created_on"),
                    Active = reader.GetBoolean("user_active")
                });
            }

            return response;
        }

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

        public async Task<Category> UpdateCategoryAsync(Category category)
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
}
