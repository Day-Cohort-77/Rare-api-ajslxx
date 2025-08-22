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
    }
  }
}
