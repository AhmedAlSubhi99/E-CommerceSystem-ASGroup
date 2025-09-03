namespace E_CommerceSystem.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }  
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;

        public bool IsExpired => DateTime.UtcNow >= Expires;

        //  Navigation property
        public User User { get; set; }
    }
}
