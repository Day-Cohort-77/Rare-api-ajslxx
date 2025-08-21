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
    }
  }
}