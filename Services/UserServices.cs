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

        // Profile Picture Methods
        public async Task<ProfilePictureResponse> UpdateProfilePictureAsync(int userId, UpdateProfilePictureRequest request)
        {
            var response = new ProfilePictureResponse
            {
                UserId = userId,
                UpdatedOn = DateTime.UtcNow
            };

            try
            {
                // Validate base64 image data
                if (string.IsNullOrWhiteSpace(request.ImageData))
                {
                    response.Success = false;
                    response.Message = "Image data is required";
                    return response;
                }

                // Validate and process base64 data
                var base64Data = request.ImageData;
                
                // Remove data URL prefix if present (e.g., "data:image/jpeg;base64,")
                if (base64Data.StartsWith("data:"))
                {
                    var commaIndex = base64Data.IndexOf(',');
                    if (commaIndex >= 0)
                    {
                        base64Data = base64Data.Substring(commaIndex + 1);
                    }
                }

                // Validate base64 format
                try
                {
                    var imageBytes = Convert.FromBase64String(base64Data);
                    
                    // Basic size validation (limit to 5MB)
                    if (imageBytes.Length > 5 * 1024 * 1024)
                    {
                        response.Success = false;
                        response.Message = "Image size cannot exceed 5MB";
                        return response;
                    }

                    // Basic image format validation by checking file headers
                    if (!IsValidImageFormat(imageBytes))
                    {
                        response.Success = false;
                        response.Message = "Invalid image format. Only JPEG, PNG, GIF, and WebP are supported";
                        return response;
                    }
                }
                catch (FormatException)
                {
                    response.Success = false;
                    response.Message = "Invalid base64 image data";
                    return response;
                }

                // Create data URL for storage
                var contentType = request.ContentType ?? GetContentTypeFromFileName(request.FileName);
                var dataUrl = $"data:{contentType};base64,{base64Data}";

                // Update user's profile image URL in database
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var updateSql = @"
                    UPDATE ""Users""
                    SET profile_image_url = @profileImageUrl
                    WHERE id = @userId AND active = true
                    RETURNING profile_image_url";

                using var command = new NpgsqlCommand(updateSql, connection);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@profileImageUrl", dataUrl);

                var result = await command.ExecuteScalarAsync();
                
                if (result != null)
                {
                    response.Success = true;
                    response.ProfileImageUrl = result.ToString()!;
                    response.FileName = request.FileName;
                    response.Message = "Profile picture updated successfully";
                }
                else
                {
                    response.Success = false;
                    response.Message = "User not found or inactive";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating profile picture: {ex.Message}";
            }

            return response;
        }

        public async Task<string?> GetProfilePictureAsync(int userId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT profile_image_url FROM \"Users\" WHERE id = @userId AND active = true";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }

        public async Task<bool> DeleteProfilePictureAsync(int userId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var updateSql = @"
                UPDATE ""Users""
                SET profile_image_url = ''
                WHERE id = @userId AND active = true";

            using var command = new NpgsqlCommand(updateSql, connection);
            command.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Helper methods for image validation
        private static bool IsValidImageFormat(byte[] imageBytes)
        {
            if (imageBytes.Length < 4) return false;

            // Check for common image file signatures
            // JPEG
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
                return true;

            // PNG
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return true;

            // GIF
            if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46)
                return true;

            // WebP
            if (imageBytes.Length >= 12 && 
                imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 && imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
                return true;

            return false;
        }

        private static string GetContentTypeFromFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg" // default
            };
        }
    }
}