namespace RareAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public int AuthorId { get; set; }

        public required string Content { get; set; }
    }
}
