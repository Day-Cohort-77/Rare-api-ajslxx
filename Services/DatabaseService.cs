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
    }
}