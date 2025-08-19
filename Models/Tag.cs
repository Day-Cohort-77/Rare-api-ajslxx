namespace RareAPI.Models
{
    public class Tag
    {
        public required int Id { get; set; }
        public required string Label { get; set; } = string.Empty;
    }
}