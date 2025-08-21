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
            var tableExists = (long)(await checkTablesCommand.ExecuteScalarAsync() ?? 0);

            //if (tableExists == 0)
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
            // Check if tag table has data
            using var tagCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Tags\"", connection);
            var tagCount = Convert.ToInt32(await tagCommand.ExecuteScalarAsync());

            if (count > 0 || tagCount > 0)
            {
                // Data already exists, no need to seed
                return;
            }
            // Insert sample data into Users table
            await ExecuteNonQueryAsync(@"
                INSERT INTO ""Users"" (first_name, last_name, email, bio, username, password, profile_image_url, created_on, active) VALUES
                ('Billy', 'Bob', 'billy@bob.com', 'I am Billy Bob', 'BillyBob', 'mycoolpass', 'https://www.thedailybeast.com/resizer/7-n47tS_FIUHO6A0UWE2XxsDki0=/arc-photo-thedailybeast/arc2-prod/public/GBJAOT4VF5IM7BLNH2I6MWRKGU.png', (TO_DATE('08/12/2025', 'MM/DD/YYYY')), true),
                ('Jimmy', 'John', 'jimmy@john.com', 'I am Jimmy John', 'JimmyJohn', 'ExcellentPassword', 'https://hips.hearstapps.com/hmg-prod/images/screenshot-2024-10-28-at-4-38-05-pm-671ff63778f27.png?crop=0.494xw:1.00xh;0.306xw,0&resize=1200:*', (TO_DATE('07/01/2022', 'MM/DD/YYYY')), true);
            ");
            // Insert sample data into Tags table
            await ExecuteNonQueryAsync(@"
                INSERT INTO ""Tags"" (label) VALUES
                ('#meme'),
                ('#fitness'),
                ('#beach');
            ");
        }
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            var response = new List<Tag>();

            using var connection = CreateConnection();
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

        public async Task<List<User>> GetAllUsersAsync()
        {
            var response = new List<User>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"SELECT * FROM ""Users"";", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.Add(new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                    LastName = reader.GetString(reader.GetOrdinal("last_name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Bio = reader.GetString(reader.GetOrdinal("bio")),
                    Username = reader.GetString(reader.GetOrdinal("username")),
                    Password = reader.GetString(reader.GetOrdinal("password")),
                    ProfileImageUrl = reader.GetString(reader.GetOrdinal("profile_image_url")),
                    CreatedOn = reader.GetDateTime(reader.GetOrdinal("created_on")),
                    Active = reader.GetBoolean(reader.GetOrdinal("active"))
                });
            }

            return response;
        }


        public async Task AddTagAsync(string label)
        {
            const string sql = @"INSERT INTO ""Tags"" (label) VALUES (@label);";

            var parameters = new Dictionary<string, object>
                {
                    { "@label", label }
                };

            await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task DeleteTagAsync(int id)
        {
            const string sql = @"DELETE FROM ""Tags"" WHERE id = @id;";

            var parameters = new Dictionary<string, object>
    {
        { "@id", id }
    };

            await ExecuteNonQueryAsync(sql, parameters);
        }


        // Add these methods to your existing DatabaseService class

        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            using var connection = CreateConnection();
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

        public async Task UpdateTagAsync(int id, string label)
{
    const string sql = @"UPDATE ""Tags"" SET label = @label WHERE id = @id;";

    var parameters = new Dictionary<string, object>
    {
        { "@id", id },
        { "@label", label }
    };

    await ExecuteNonQueryAsync(sql, parameters);
}

    }
}
