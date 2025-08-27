using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class UserEndpoints
  {
    public static void MapUserEndpoints(this WebApplication app)
    {
      app.MapGet("/users", async (UserServices userService) =>
      {
        var users = await userService.GetAllUsersAsync();
        return Results.Ok(users);
      });

      app.MapGet("/users/{id}", async (int id, UserServices userService) =>
      {
        var user = await userService.GetUserByIdAsync(id);
        return user is not null ? Results.Ok(user) : Results.NotFound();
      });

      app.MapPut("/users/{id}", async (int id, User updatedUser, UserServices userService) =>
      {
        var user = await userService.UpdateUserAsync(id, updatedUser);
        return user is not null ? Results.Ok(user) : Results.NotFound();
      });

      app.MapDelete("/users/{id}", async (int id, UserServices userService) =>
      {
        var success = await userService.DeleteUserAsync(id);
        return success ? Results.NoContent() : Results.NotFound();
      });

      // Profile Picture Endpoints
      app.MapPost("/users/{userId}/profile-picture", async (int userId, UpdateProfilePictureRequest request, UserServices userService) =>
      {
        try
        {
          if (string.IsNullOrWhiteSpace(request.ImageData) || string.IsNullOrWhiteSpace(request.FileName))
          {
            return Results.BadRequest("ImageData and FileName are required");
          }

          var result = await userService.UpdateProfilePictureAsync(userId, request);
          
          if (result.Success)
          {
            return Results.Ok(result);
          }
          else
          {
            return Results.BadRequest(result.Message);
          }
        }
        catch (Exception ex)
        {
          return Results.Problem($"Error updating profile picture: {ex.Message}");
        }
      });

      app.MapGet("/users/{userId}/profile-picture", async (int userId, UserServices userService) =>
      {
        try
        {
          var profilePictureUrl = await userService.GetProfilePictureAsync(userId);
          
          if (string.IsNullOrEmpty(profilePictureUrl))
          {
            return Results.NotFound("Profile picture not found");
          }

          return Results.Ok(new { userId, profileImageUrl = profilePictureUrl });
        }
        catch (Exception ex)
        {
          return Results.Problem($"Error retrieving profile picture: {ex.Message}");
        }
      });

      app.MapDelete("/users/{userId}/profile-picture", async (int userId, UserServices userService) =>
      {
        try
        {
          var success = await userService.DeleteProfilePictureAsync(userId);
          return success ? Results.NoContent() : Results.NotFound("User not found");
        }
        catch (Exception ex)
        {
          return Results.Problem($"Error deleting profile picture: {ex.Message}");
        }
      });
    }
  }
}
