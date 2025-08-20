namespace RareAPI.Models
{
  public class Post
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public required string Title { get; set; }
    public DateTime PublicationDate { get; set; }
    public required string ImageUrl { get; set; }
    public required string Content { get; set; }
    public bool Approved { get; set; }
  }
}
