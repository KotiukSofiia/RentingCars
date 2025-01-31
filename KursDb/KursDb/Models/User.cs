namespace KursDb.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; } // "Admin" or "User"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
