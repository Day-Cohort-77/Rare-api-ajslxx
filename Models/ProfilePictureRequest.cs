namespace RareAPI.Models
{
    public class UpdateProfilePictureRequest
    {
        public required string ImageData { get; set; } // Base64 encoded image
        public required string FileName { get; set; }  // Original filename for reference
        public string? ContentType { get; set; }       // MIME type (e.g., "image/jpeg")
    }

    public class ProfilePictureResponse
    {
        public int UserId { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UpdatedOn { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
