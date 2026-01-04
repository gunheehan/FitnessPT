using FitnessPT.Models;

namespace FitnessPT.Services;

public class AlertService
{
    private AlertState _state = new();
    public event Action? OnStateChanged;

    public AlertState State => _state;

    // 기본 Alert (확인 버튼만)
    public void Show(string message, string title = "알림", AlertType type = AlertType.Info, Action? onConfirm = null)
    {
        _state = new AlertState
        {
            IsVisible = true,
            Title = title,
            Message = message,
            Type = type,
            ShowCancelButton = false,
            OnConfirm = onConfirm ?? (() => Hide())
        };
        OnStateChanged?.Invoke();
    }

    // Success Alert
    public void ShowSuccess(string message, string title = "성공", Action? onConfirm = null)
    {
        Show(message, title, AlertType.Success, onConfirm);
    }

    // Error Alert
    public void ShowError(string message, string title = "오류", Action? onConfirm = null)
    {
        Show(message, title, AlertType.Error, onConfirm);
    }

    // Warning Alert
    public void ShowWarning(string message, string title = "경고", Action? onConfirm = null)
    {
        Show(message, title, AlertType.Warning, onConfirm);
    }

    // Info Alert
    public void ShowInfo(string message, string title = "알림", Action? onConfirm = null)
    {
        Show(message, title, AlertType.Info, onConfirm);
    }

    // Confirm (확인/취소 버튼)
    public void Confirm(
        string message, 
        Action onConfirm, 
        Action? onCancel = null,
        string title = "확인",
        string confirmText = "확인",
        string cancelText = "취소",
        AlertType type = AlertType.Question)
    {
        Console.WriteLine("Set Confirm");
        _state = new AlertState
        {
            IsVisible = true,
            Title = title,
            Message = message,
            Type = type,
            ShowCancelButton = true,
            ConfirmButtonText = confirmText,
            CancelButtonText = cancelText,
            OnConfirm = () => 
            {
                onConfirm?.Invoke();
                Hide();
            },
            OnCancel = () => 
            {
                onCancel?.Invoke();
                Hide();
            }
        };

        OnStateChanged?.Invoke();
    }

    // 삭제 확인 (위험 스타일)
    public void ConfirmDelete(string message, Action onConfirm, Action? onCancel = null, string title = "삭제 확인")
    {
        Confirm(
            message,
            onConfirm,
            onCancel,
            title,
            "삭제",
            "취소",
            AlertType.Error
        );
    }

    public void Hide()
    {
        _state = new AlertState { IsVisible = false };
        OnStateChanged?.Invoke();
    }
}