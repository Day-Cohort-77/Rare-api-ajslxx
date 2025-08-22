using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            // POST /register
            app.MapPost("/register", async (User newUser, UserServices userService) =>
            {
                try
                {
                    // Check if user already exists
                    if (await userService.UserExistsAsync(newUser.Email))
                    {
                        return Results.BadRequest(new { message = "User with this email already exists" });
                    }

                    // Create new user
                    var createdUser = await userService.CreateUserAsync(newUser);
                    if (createdUser != null)
                    {
                        return Results.Created($"/users/{createdUser.Id}", createdUser);
                    }

                    return Results.BadRequest(new { message = "Failed to create user" });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            // POST /login
            app.MapPost("/login", async (LoginRequest loginRequest, UserServices userService) =>
            {
                try
                {
                    var (userId, storedPassword) = await userService.GetUserCredentialsAsync(loginRequest.Email);

                    if (userId.HasValue && storedPassword != null)
                    {
                        // Simple password comparison (in production, use proper password hashing)
                        if (storedPassword == loginRequest.Password)
                        {
                            return Results.Ok(new { valid = true, token = userId.Value });
                        }
                    }

                    return Results.Ok(new { valid = false, token = (int?)null });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}