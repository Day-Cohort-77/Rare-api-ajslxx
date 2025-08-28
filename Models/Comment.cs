namespace RareAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public int AuthorId { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
        
        // Navigation/Display properties (not stored in DB, used for joined queries)
        public string AuthorDisplayName { get; set; } = string.Empty;

        public string PostTitle { get; set; } = string.Empty;
    }

    public class PostCommentsResponse
    {
        public Post Post { get; set; }
        public List<CommentWithAuthor> Comments { get; set; }
    }

    public class CommentWithAuthor : Comment
    {
        public string AuthordisplayName { get; set; } = string.Empty;

    }

    public class CommentWithDetails
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorDisplayName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public int PostId { get; set; }
    }

}
