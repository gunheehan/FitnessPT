using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace FitnessPT.Components.Modals;

public partial class PasswordModal : ComponentBase
{
    private readonly string Pin = "0305";
    [Parameter] public EventCallback OnAuthenticated { get; set; } // 검증 성공 시 호출
    [Parameter] public EventCallback OnClose { get; set; } // 모달 닫기

    private string[] pinInputs = new string[4]; // 각 입력 칸에 입력된 PIN 값 저장
    private bool invalidPin = false; // 초기값은 false로 설정
    private bool isPinComplete = false; // 모든 입력 칸이 채워졌는지 여부를 확인하는 플래그
    private ElementReference[] pinElements = new ElementReference[4]; // 각 입력 필드 참조

    private string title;
    private string description;
    private string submitText;
    private string errorMessage;
    
    private void CloseModal(MouseEventArgs e)
    {
        Console.WriteLine("Close Call");
        OnClose.InvokeAsync();
    }
    protected override void OnInitialized()
    {
        for (int i = 0; i < pinInputs.Length; i++)
        {
            pinInputs[i] = string.Empty; // 초기화
        }

        title = "4자리 비밀번호를 입력하세요";
        description = "관리자가 비밀번호를 알고 있어요!";
        submitText = "입력하기";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // 첫 번째 입력 필드에 포커스
            await pinElements[0].FocusAsync();
        }
    }

    private async Task OnInputChanged(ChangeEventArgs e, int boxIndex)
    {
        var value = e.Value?.ToString();
        if (value.Length == 1 && (value[0] < '0' || value[0] > '9'))
        {
            invalidPin = true;
            errorMessage = "숫자만 입력이 가능합니다.";
            return;
        }

        invalidPin = false;
        // 입력이 숫자이고, 입력된 값이 있을 경우 처리
        if (!string.IsNullOrEmpty(value) && value.Length == 1 && char.IsDigit(value[0]))
        {
            pinInputs[boxIndex] = value; // 입력값 저장
            if (boxIndex < 3) // 마지막 입력 필드가 아니라면 다음 필드로 포커스 이동
            {
                await pinElements[boxIndex + 1].FocusAsync();
            }
        }
        else
        {
            pinInputs[boxIndex] = string.Empty; // 잘못된 값일 경우 입력 초기화
        }

        // 모든 입력 필드가 채워졌는지 확인
        isPinComplete = pinInputs.All(pin => !string.IsNullOrEmpty(pin));

        // AutoSubmit이 활성화되어 있고 모든 입력이 완료되었으면 처리
        if (isPinComplete)
        {
            SubmitPin();
        }
    }

    private async Task OnKeyUp(KeyboardEventArgs e, int boxIndex)
    {
        if (e.Key == "Backspace" && boxIndex > 0) // 백스페이스 입력 시
        {
            await pinElements[boxIndex - 1].FocusAsync(); // 이전 입력 필드로 포커스 이동
            pinInputs[boxIndex] = string.Empty; // 현재 필드 값 초기화
        }

        if (e.Key == "Enter")
        {
            SubmitPin();
        }

        // 입력 필드 상태를 다시 확인
        isPinComplete = pinInputs.All(pin => !string.IsNullOrEmpty(pin));
    }

    // PIN 검증 또는 생성 처리
    private async Task SubmitPin()
    {
        string enteredPin = string.Join("", pinInputs); // 입력된 PIN 결합

        if (isPinComplete) // 4자리가 다 입력된 경우에만 처리
        {
            if (string.IsNullOrEmpty(Pin))
            {
                throw new InvalidOperationException("Pin is required in Validate mode.");
            }

            invalidPin = Pin != enteredPin; // 입력된 PIN과 전달된 PIN 비교
            if (!invalidPin)
            {
                OnAuthenticated.InvokeAsync(true); // 인증 성공 시 이벤트 호출
            }
            else
            {
                errorMessage = "비밀번호가 일치하지 않습니다.";
            }
        }

        StateHasChanged(); // 상태 변경 반영
    }
}