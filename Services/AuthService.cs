using FitnessPT.Models;

namespace FitnessPT.Services;

public class AuthService
{
    private readonly HttpClient client;
    private readonly ILogger<AuthService> logger;
    private readonly IConfiguration configuration;

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient, ILogger<AuthService> log, IConfiguration config)
    {
        client = httpClient;
        logger = log;
        configuration = config;
    }

    public async Task<GoogleAuthResult> LoginWithGoogleAsync(string googleToken)
    {
        try
        {
            var apiUrl = configuration["ApiSettings:BaseUrl"];
            var request = new GoogleAuthRequest { GoogleToken = googleToken };
            var response = await client.PostAsJsonAsync($"{apiUrl}/api/GoogleAuth/login", request);
            var result = await response.Content.ReadFromJsonAsync<GoogleAuthResult>();

            if (result?.Success == true && result.User != null)
            {
                CurrentUser = result.User;
                logger.LogInformation("로그인 성공: {Email}", result.User.Email);
                OnAuthStateChanged?.Invoke();
                
                return result;
            }

            return new GoogleAuthResult()
            {
                Success = false,
                ErrorMessage = "No Signer"
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "로그인 실패");
            return new GoogleAuthResult 
            { 
                Success = false, 
                ErrorMessage = $"로그인 실패: {e.Message}" 
            };
        }
    }
    public Task LogoutAsync()
    {
        CurrentUser = null;
        OnAuthStateChanged?.Invoke();
        return Task.CompletedTask;
    }
}