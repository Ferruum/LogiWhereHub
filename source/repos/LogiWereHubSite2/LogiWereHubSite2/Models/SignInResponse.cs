namespace LogiWereHubSite2.Models
{
    public class SignInResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } // Добавляем свойство Role
        public bool EmailConfirmed { get; set; }
        public string Email { get; set; }
    }

}
