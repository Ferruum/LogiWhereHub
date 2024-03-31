namespace LogiWereHubSite2.Models
{
    public class UserModel
    {
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; } // Новое свойство для электронной почты
        public string? Role { get; set; } // Новое свойство для роли пользователя
        public bool EmailConfirmed { get; set; }
    }
}
