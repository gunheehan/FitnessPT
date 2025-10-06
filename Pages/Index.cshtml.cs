using FitnessPT.Services;

namespace FitnessPT.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly AuthService _authService;
    
    // 뷰에서 사용할 속성들
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    
    public IndexModel(AuthService authService)
    {
        _authService = authService;
    }
    
    // 페이지 로드 시 실행
    public void OnGet()
    {
        IsAuthenticated = _authService.IsAuthenticated;
        UserName = _authService.CurrentUser?.Name;
    }
}