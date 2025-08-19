namespace RareAPI.Models
{
  public class PostReaction
  {
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required int ReactionId { get; set; }
    public required int PostId { get; set; }
  }
}