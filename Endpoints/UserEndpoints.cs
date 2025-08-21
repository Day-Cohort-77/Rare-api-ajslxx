using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class UserEndpoints
  {
    public static void MapUserEndpoints(this WebApplication app)
    {
      app.MapGet("/users", async (DatabaseService db) =>
      {
        var users = await db.GetAllUsersAsync();
        return Results.Ok(users);
      });

      app.MapGet("/users/{id}", async (int id, DatabaseService db) =>
      {
        var user = await db.GetUserByIdAsync(id);
        return user is not null ? Results.Ok(user) : Results.NotFound();
      });
    }
  }
}
