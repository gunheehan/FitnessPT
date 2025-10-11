using FitnessPT.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FitnessPT.Services;

public class AuthService
{
    private readonly HttpClient client;
    private readonly ProtectedSessionStorage sessionStorage; // 로그인 정보 저장용 세션
    private readonly ILogger<AuthService> logger;
    private readonly IConfiguration configuration;

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public event Action<User>? OnAuthStateChanged;

    private bool initialized = false;
    private const string USER_KEY = "fitness_user";

    public AuthService(HttpClient httpClient, ProtectedSessionStorage session, ILogger<AuthService> log, IConfiguration config)
    {
        client = httpClient;
        sessionStorage = session;
        logger = log;
        configuration = config;
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            var result = await sessionStorage.GetAsync<User>(USER_KEY);
            
            if (result.Success && result.Value != null)
            {
                CurrentUser = result.Value;
                logger.LogInformation("세션에서 유저 정보 복원: {Email}", CurrentUser.Email);
                OnAuthStateChanged?.Invoke(CurrentUser);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "세션 복원 실패");
        }
        finally
        {
            initialized = true;
        }
    }

    public async Task<GoogleAuthResult> LoginWithGoogleAsync(string googleToken)
    {
        try
        {
            var apiUrl = configuration["FITNESSPT:ApiSettings:BaseUrl"];
            var request = new GoogleAuthRequest { GoogleToken = googleToken };
            var response = await client.PostAsJsonAsync($"{apiUrl}/api/GoogleAuth/login", request);
            var result = await response.Content.ReadFromJsonAsync<GoogleAuthResult>();

            if (result?.Success == true && result.User != null)
            {
                CurrentUser = result.User;
                sessionStorage.SetAsync(USER_KEY, result.User);
                logger.LogInformation("로그인 성공: {Email}", result.User.Email);
                OnAuthStateChanged?.Invoke(CurrentUser);
                
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
        sessionStorage.SetAsync(USER_KEY, null);
        OnAuthStateChanged?.Invoke(CurrentUser);
        return Task.CompletedTask;
    }
}