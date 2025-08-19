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
    // First, create the database if it doesn't exist
    using var connection = new NpgsqlConnection(_connectionString.Replace("Database=RareAPI", "Database=postgres"));
    await connection.OpenAsync();

    // Check if database exists
    using var checkCommand = new NpgsqlCommand(
        "SELECT 1 FROM pg_database WHERE datname = 'rareapi'",
        connection);
    var exists = await checkCommand.ExecuteScalarAsync();

    if (exists == null)
    {
        // Create the database
        using var createDbCommand = new NpgsqlCommand(
            "CREATE DATABASE rareapi",
            connection);
        await createDbCommand.ExecuteNonQueryAsync();
    }

    // Now connect to the harbormaster database and create tables
    string sql = File.ReadAllText("database-setup.sql");
    await ExecuteNonQueryAsync(sql);
}
    }
}