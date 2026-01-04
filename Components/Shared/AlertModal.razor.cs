using FitnessPT.Models;
using Microsoft.AspNetCore.Components;

namespace FitnessPT.Components.Shared;

public partial class AlertModal : ComponentBase
{
    private AlertState alertState => AlertService.State;

    protected override void OnInitialized()
    {
        AlertService.OnStateChanged += StateHasChanged;
        Console.WriteLine("AlertModal 초기화됨");
    }

    private string IconEmoji => alertState.Type switch
    {
        AlertType.Success => "✅",
        AlertType.Error => "❌",
        AlertType.Warning => "⚠️",
        AlertType.Info => "ℹ️",
        AlertType.Question => "❓",
        _ => "ℹ️"
    };

    private string ConfirmButtonClass => alertState.Type switch
    {
        AlertType.Error => "btn-danger",
        AlertType.Success => "btn-success",
        AlertType.Warning => "btn-warning",
        _ => "btn-primary"
    };

    private void HandleConfirm()
    {
        Console.WriteLine("Confirm 버튼 클릭");
        alertState.OnConfirm?.Invoke();
    }

    private void HandleCancel()
    {
        Console.WriteLine("Cancel 버튼 클릭");
        alertState.OnCancel?.Invoke();
    }

    private void HandleOverlayClick()
    {
// 오버레이 클릭 시 동작 (선택사항)
    }

    public void Dispose()
    {
        AlertService.OnStateChanged -= StateHasChanged;
        Console.WriteLine("AlertModal Disposed");
    }
}