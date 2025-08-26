using RareAPI.Models;
using RareAPI.Services;

namespace RareAPI.Endpoints
{
  public static class SubscriptionEndpoints
  {
    public static void MapSubscriptionEndpoints(this WebApplication app)
    {
      app.MapGet("/subscriptions/{id}", async (int id, SubscriptionService subscriptionService) =>
      {
        var subscription = await subscriptionService.GetSubscriptionByIdAsync(id);
        return subscription is not null ? Results.Ok(subscription) : Results.NotFound();
      });
    }
  }
}
