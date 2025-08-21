using Npgsql;
using RareAPI.Models;

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
            
            var masterConnStr = _connectionString.Replace("Database=RareAPI", "Database=postgres");
            using var masterConnection = new NpgsqlConnection(masterConnStr);
            await masterConnection.OpenAsync();

            
            using var checkCommand = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = 'RareAPI'",
                masterConnection);
            var exists = await checkCommand.ExecuteScalarAsync();

            if (exists == null)
            {
                
                using var createDbCommand = new NpgsqlCommand(
                    "CREATE DATABASE \"RareAPI\"",
                    masterConnection);
                await createDbCommand.ExecuteNonQueryAsync();
            }

            
            await masterConnection.CloseAsync();

            
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
            
            using var connection = CreateConnection();
            await connection.OpenAsync();

            
            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", connection);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            // Check if tag table has data
            using var tagCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Tags\"", connection);
            var tagCount = Convert.ToInt32(await tagCommand.ExecuteScalarAsync());

            if (count > 0 || tagCount > 0)
            {
                
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
                    Id = reader.GetInt32(0),                    // user_id
                    FirstName = reader.GetString(1),            // user_first_name
                    LastName = reader.GetString(2),             // user_last_name
                    Email = reader.GetString(3),                // user_email
                    Bio = reader.GetString(4),                  // user_bio
                    Username = reader.GetString(5),             // user_username
                    Password = reader.GetString(6),             // user_password
                    ProfileImageUrl = reader.GetString(7),      // user_profile_image_url
                    CreatedOn = reader.GetDateTime(8),          // user_created_on
                    Active = reader.GetBoolean(9)               // user_active
                });
            }

            return response;
        }

       
    }
}


