using InstagramClonAPI.Models;

public class RefreshToken
{
    public int Id { get; set; }  // Primary key
    public string Token { get; set; }
    public int UserId { get; set; }
    public DateTime Expiration { get; set; }

    public User User { get; set; }  // Relación opcional
}
