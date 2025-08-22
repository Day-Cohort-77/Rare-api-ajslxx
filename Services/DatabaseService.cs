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


            

            
            using var checkTablesCommand = new NpgsqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Users'",
                rareApiConnection);
            var tableExists = (long)(await checkTablesCommand.ExecuteScalarAsync() ?? 0);

            
            {
                
                string sql = File.ReadAllText("database-setup.sql");
                using var setupCommand = new NpgsqlCommand(sql, rareApiConnection);
                await setupCommand.ExecuteNonQueryAsync();
            }

        }

        public async Task SeedDatabaseAsync()
        {
            
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Check individual tables and seed each separately
            
            // Seed Users if empty
            using var userCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", connection);
            var userCount = Convert.ToInt32(await userCommand.ExecuteScalarAsync());
            
            if (userCount == 0)
            {
                await ExecuteNonQueryAsync(@"
                    INSERT INTO ""Users"" (first_name, last_name, email, bio, username, password, profile_image_url, created_on, active) VALUES
                    ('test', 'test', 'test@test.com', 'test', 'test', 'test', 'test', (TO_DATE('08/12/2025', 'MM/DD/YYYY')), true),
                    ('test2', 'test2', 'test2@test.com', 'test2', 'test2', 'test2', 'test2', (TO_DATE('08/02/2025', 'MM/DD/YYYY')), true),
                    ('test3', 'test3', 'test3@test.com', 'test3', 'test3', 'test3', 'test3', (TO_DATE('08/01/2025', 'MM/DD/YYYY')), true)
                    ");
            }

            // Seed Tags if empty
            using var tagCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Tags\"", connection);
            var tagCount = Convert.ToInt32(await tagCommand.ExecuteScalarAsync());
            
            if (tagCount == 0)
            {
                await ExecuteNonQueryAsync(@"
                    INSERT INTO ""Tags"" (label) VALUES
                    ('#meme'),
                    ('#fitness'),
                    ('#beach');
                ");
            }

            // Seed Categories if empty
            using var categoryCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Categories\"", connection);
            var categoryCount = Convert.ToInt32(await categoryCommand.ExecuteScalarAsync());
            
            if (categoryCount == 0)
            {
                await ExecuteNonQueryAsync(@"
                    INSERT INTO ""Categories"" (label) VALUES
                    ('News'),
                    ('Sports'),
                    ('Entertainment'),
                    ('Gaming'),
                    ('Music'),
                    ('Movies');
                ");
            }

            // Seed Posts if empty
            using var postCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Posts\"", connection);
            var postCount = Convert.ToInt32(await postCommand.ExecuteScalarAsync());
            
            if (postCount == 0)
            {
                await ExecuteNonQueryAsync(@"
                    INSERT INTO ""Posts"" (user_id, category_id, title, publication_date, image_url, content, approved) VALUES
                    (1, 1, 'First Post', '2025-08-12'::DATE, 'https://example.com/image1.png', 'Hello World! This is my first post.', true),
                    (2, 2, 'Second Post', '2022-07-01'::DATE, 'https://example.com/image2.png', 'Another post about sports!', true),
                    (1, 3, 'Entertainment News', '2025-08-11'::DATE, 'https://example.com/image3.png', 'Latest entertainment updates.', false);
                ");
            }

            // Seed Comments if empty  
            using var commentCommand = new NpgsqlCommand("SELECT COUNT(*) FROM \"Comments\"", connection);
            var commentCount = Convert.ToInt32(await commentCommand.ExecuteScalarAsync());
            
            if (commentCount == 0)
            {
                await ExecuteNonQueryAsync(@"
                    INSERT INTO ""Comments"" (post_id, author_id, content) VALUES
                    (1, 2, 'Great first post!'),
                    (1, 1, 'Thanks for the comment!'),
                    (2, 1, 'Love this sports content.');
                ");
            }
        }

        

       
    }
}


