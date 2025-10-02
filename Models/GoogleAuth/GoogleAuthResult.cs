namespace FitnessPT.Models;

public class GoogleAuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public User? User { get; set; }
    public bool IsNewUser { get; set; }
    public string? ErrorMessage { get; set; }
}