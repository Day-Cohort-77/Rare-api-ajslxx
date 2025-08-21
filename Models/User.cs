namespace RareAPI.Models
{
    public class User
    {
        public int Id { get; set; }



        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Bio { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string ProfileImageUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool Active { get; set; }
    }
}
