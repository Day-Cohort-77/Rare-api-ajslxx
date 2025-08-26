namespace RareAPI.Models
{
    public class AddPostReactionRequest
    {
        public int UserId { get; set; }
        public int ReactionId { get; set; }
        public int PostId { get; set; }
    }

    public class ReactionCount
    {
        public int ReactionId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PostWithReactions
    {
        public Post Post { get; set; } = new() { Title = "", ImageUrl = "", Content = "" };
        public List<ReactionCount> ReactionCounts { get; set; } = new();
        public List<int> UserReactions { get; set; } = new(); // Reaction IDs that the current user has used
    }
}
