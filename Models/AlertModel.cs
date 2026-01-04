namespace FitnessPT.Models;

public class AlertState
{
    public bool IsVisible { get; set; }
    public string Title { get; set; } = "알림";
    public string Message { get; set; } = "";
    public AlertType Type { get; set; } = AlertType.Info;
    public bool ShowCancelButton { get; set; } = false;
    public string ConfirmButtonText { get; set; } = "확인";
    public string CancelButtonText { get; set; } = "취소";
    public Action OnConfirm { get; set; } = () => { };
    public Action OnCancel { get; set; } = () => { };
}

public enum AlertType
{
    Success,
    Error,
    Warning,
    Info,
    Question
}