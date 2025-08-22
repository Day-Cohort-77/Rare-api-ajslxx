using Npgsql;
using RareAPI.Models;

namespace RareAPI.Services
{
    public class UserServices
    {
        private readonly DatabaseService _databaseService;

        public UserServices(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private NpgsqlConnection CreateConnection()
        {
            return _databaseService.CreateConnection();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var response = new List<User>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
              @"SELECT 
                    id, 
                    first_name,
                    last_name, 
                    email, 
                    bio, 
                    username, 
                    password, 
                    profile_image_url,
                    created_on,
                    active
                    FROM ""Users"";",
               connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.Add(new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Bio = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    ProfileImageUrl = reader.GetString(7),
                    CreatedOn = reader.GetDateTime(8),
                    Active = reader.GetBoolean(9)
                });
            }

            return response;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT 
                    id, 
                    first_name,
                    last_name, 
                    email, 
                    bio, 
                    username, 
                    password, 
                    profile_image_url,
                    created_on,
                    active
                    FROM ""Users"" WHERE id = @id;",
                connection);

            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Bio = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    ProfileImageUrl = reader.GetString(7),
                    CreatedOn = reader.GetDateTime(8),
                    Active = reader.GetBoolean(9)
                };
            }

            return null;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\" WHERE email = @email", connection);
            command.Parameters.AddWithValue("@email", email);

            var result = await command.ExecuteScalarAsync();
            return result != null && (long)result > 0;
        }

        public async Task<User?> CreateUserAsync(User newUser)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var insertSql = @"
                INSERT INTO ""Users"" (first_name, last_name, email, bio, username, password, profile_image_url, created_on, active)
                VALUES (@firstName, @lastName, @email, @bio, @username, @password, @profileImageUrl, @createdOn, @active)
                RETURNING id, first_name, last_name, email, bio, username, password, profile_image_url, created_on, active";

            using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@firstName", newUser.FirstName);
            command.Parameters.AddWithValue("@lastName", newUser.LastName);
            command.Parameters.AddWithValue("@email", newUser.Email);
            command.Parameters.AddWithValue("@bio", newUser.Bio ?? string.Empty);
            command.Parameters.AddWithValue("@username", newUser.Username);
            command.Parameters.AddWithValue("@password", newUser.Password);
            command.Parameters.AddWithValue("@profileImageUrl", newUser.ProfileImageUrl ?? string.Empty);
            command.Parameters.AddWithValue("@createdOn", DateTime.UtcNow);
            command.Parameters.AddWithValue("@active", true);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Bio = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    ProfileImageUrl = reader.GetString(7),
                    CreatedOn = reader.GetDateTime(8),
                    Active = reader.GetBoolean(9)
                };
            }

            return null;
        }

        public async Task<(int? userId, string? password)> GetUserCredentialsAsync(string email)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT id, password FROM \"Users\" WHERE email = @email AND active = true";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (reader.GetInt32(0), reader.GetString(1));
            }

            return (null, null);
        }

        public async Task<User?> UpdateUserAsync(int id, User updatedUser)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var updateSql = @"
                UPDATE ""Users""
                SET first_name = @firstName, last_name = @lastName, email = @email, 
                    bio = @bio, username = @username, profile_image_url = @profileImageUrl
                WHERE id = @id
                RETURNING id, first_name, last_name, email, bio, username, password, profile_image_url, created_on, active";

            using var command = new NpgsqlCommand(updateSql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@firstName", updatedUser.FirstName);
            command.Parameters.AddWithValue("@lastName", updatedUser.LastName);
            command.Parameters.AddWithValue("@email", updatedUser.Email);
            command.Parameters.AddWithValue("@bio", updatedUser.Bio ?? string.Empty);
            command.Parameters.AddWithValue("@username", updatedUser.Username);
            command.Parameters.AddWithValue("@profileImageUrl", updatedUser.ProfileImageUrl ?? string.Empty);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Bio = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    ProfileImageUrl = reader.GetString(7),
                    CreatedOn = reader.GetDateTime(8),
                    Active = reader.GetBoolean(9)
                };
            }

            return null;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var deleteSql = "UPDATE \"Users\" SET active = false WHERE id = @id";
            using var command = new NpgsqlCommand(deleteSql, connection);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}