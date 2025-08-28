using Npgsql;
using RareAPI.Models;

namespace RareAPI.Services
{
  public class SubscriptionService : DatabaseService
  {
    public SubscriptionService(IConfiguration config) : base(config) { }

    public async Task<Subscription?> GetSubscriptionByIdAsync(int id)
    {
      using var connection = CreateConnection();
      await connection.OpenAsync();

      using var command = new NpgsqlCommand(
          @"SELECT ""id"", ""follower_id"", ""author_id"", ""created_on"" FROM ""Subscriptions"" WHERE ""id"" = @id",
          connection);
      command.Parameters.AddWithValue("@id", id);

      using var reader = await command.ExecuteReaderAsync();

      if (await reader.ReadAsync())
      {
        return new Subscription
        {
          Id = reader.GetInt32(0),
          FollowerId = reader.GetInt32(1),
          AuthorId = reader.GetInt32(2),
          CreatedOn = reader.GetDateTime(3)
        };
      }
      return null;
    }

        public async Task<int?> GetTotalSubscriptionByAuthorIdAsync(int id)
    {
      using var connection = CreateConnection();
      await connection.OpenAsync();

      using var command = new NpgsqlCommand(
          @"SELECT 
          COUNT(*)
          FROM ""Subscriptions""
          WHERE ""author_id"" = @id",
          connection);
      command.Parameters.AddWithValue("@id", id);

      using var reader = await command.ExecuteReaderAsync();

      if (await reader.ReadAsync())
      {
        var totalSubs = reader.GetInt32(0);
        return totalSubs;
      }
      return null;
    }
  }
}
