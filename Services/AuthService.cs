using FitnessPT.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FitnessPT.Services;

public class AuthService
{
    private readonly HttpClient client;
    private readonly ProtectedSessionStorage sessionStorage;
    private readonly ILogger<AuthService> logger;
    private readonly IConfiguration configuration;

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    
    public event Action<User?>? OnAuthStateChanged;

    private bool initialized = false;
    private const string USER_KEY = "fitness_user";

    public AuthService(
        HttpClient httpClient, 
        ProtectedSessionStorage session, 
        ILogger<AuthService> log, 
        IConfiguration config)
    {
        client = httpClient;
        sessionStorage = session;
        logger = log;
        configuration = config;
    }
    
    public async Task InitializeAsync()
    {
        if (initialized)
            return;
        
        try
        {
            // 세션 정보 불러오기
            var result = await sessionStorage.GetAsync<User>(USER_KEY);
            
            if (result.Success && result.Value != null)
            {
                CurrentUser = result.Value;
                OnAuthStateChanged?.Invoke(CurrentUser);
            }
            else
            {
                OnAuthStateChanged?.Invoke(null);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Session Replace failed");
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
            logger.LogInformation("Start Goole Login");

            var apiUrl = configuration["FITNESSPT:ApiSettings:BaseUrl"];
            var request = new GoogleAuthRequest { GoogleToken = googleToken };
            
            var response = await client.PostAsJsonAsync($"{apiUrl}/api/GoogleAuth/login", request);
            
            if (!response.IsSuccessStatusCode)
            {
                return new GoogleAuthResult
                {
                    Success = false,
                    ErrorMessage = $"API 오류: {response.StatusCode}"
                };
            }
            
            var result = await response.Content.ReadFromJsonAsync<GoogleAuthResult>();

            if (result?.Success == true && result.User != null)
            {
                CurrentUser = result.User;
                
                // 세션 스토리지 저장
                await sessionStorage.SetAsync(USER_KEY, result.User);
                OnAuthStateChanged?.Invoke(CurrentUser);
                
                return result;
            }

            return new GoogleAuthResult
            {
                Success = false,
                ErrorMessage = "인증 정보가 올바르지 않습니다"
            };
        }
        catch (Exception e)
        {
            return new GoogleAuthResult 
            { 
                Success = false, 
                ErrorMessage = $"로그인 실패: {e.Message}" 
            };
        }
    }
    
    public async Task LogoutAsync()
    {
        try
        {
            logger.LogInformation("Start Logout");

            CurrentUser = null;
            
            // 세션 스토리지 삭제
            await sessionStorage.DeleteAsync(USER_KEY);
            
            OnAuthStateChanged?.Invoke(null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Logout Error");
        }
    }
}