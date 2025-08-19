namespace RareAPI.Models
{
  public class Subscription
  {
    public int Id { get; set; }
    public required int FollowerId { get; set; }
    public required int AuthorId { get; set; }
    public DateTime CreatedOn { get; set; }
  }
}





