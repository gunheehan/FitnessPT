using FitnessPT.Dtos;
using FitnessPT.Models;
using Microsoft.AspNetCore.Components;

namespace FitnessPT.Components.Modals;

public partial class RoutineModal : ComponentBase
{
    [Parameter, EditorRequired] public RoutineDto Routine { get; set; } = new();
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public ModalState state { get; set; }
    [Parameter] public EventCallback<RoutineInfoDto> OnSave { get; set; }
    [Parameter] public EventCallback<bool> IsActiveChanged { get; set; }

    private List<RoutineExerciseDto> exerciseList = new();

    protected override async Task OnParametersSetAsync()
    {
        // Routine이 변경될 때마다 운동 목록 로드
        if (Routine?.Id > 0 && (state == ModalState.EDIT || state == ModalState.PREVIEW))
        {
            var result = await Controller.GetRoutineDetail(Routine.Id);
            exerciseList = result.ExerciseInfo;
        }
        else if (state != ModalState.EDIT && state != ModalState.PREVIEW)
        {
            exerciseList = new List<RoutineExerciseDto>();
        }
    }

    private void SwitchToEditMode()
    {
        state = ModalState.EDIT;
        StateHasChanged();
    }

    private async Task ApplyFilters()
    {
        Console.WriteLine($"필터 적용 - 레벨: {Routine.Level}, 카테고리: {Routine.Category}");
    }

    private bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Routine.Name) &&
               !string.IsNullOrWhiteSpace(Routine.Level) &&
               !string.IsNullOrWhiteSpace(Routine.Category) &&
               exerciseList.Any();
    }

    private async Task HandleSave()
    {
        if (IsValid())
        {
            var newInfo = new RoutineInfoDto
            {
                Name = Routine.Name,
                Level = Routine.Level,
                Category = Routine.Category,
                ExerciseInfo = exerciseList
            };

            await OnSave.InvokeAsync(newInfo);
        }
    }

    private void AddExercise(ExerciseDto exercise)
    {
        if (exerciseList.Any(e => e.ExerciseId == exercise.Id))
        {
            Console.WriteLine("이미 추가된 운동입니다.");
            return;
        }

        var newExercise = new RoutineExerciseDto
        {
            RoutineId = Routine.Id,
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
            OrderIndex = exerciseList.Count + 1,
            Sets = 3,
            Reps = 12,
            DurationSeconds = null,
            RestSeconds = 60
        };

        exerciseList.Add(newExercise);
        StateHasChanged();
    }

    private void RemoveExercise(int index)
    {
        if (index >= 0 && index < exerciseList.Count)
        {
            exerciseList.RemoveAt(index);

            for (int i = 0; i < exerciseList.Count; i++)
            {
                exerciseList[i].OrderIndex = i + 1;
            }

            StateHasChanged();
        }
    }

    private void OnClose()
    {
        IsActiveChanged.InvokeAsync(false);
    }

    private string GetLevelText()
    {
        return Routine.Level switch
        {
            "beginner" => "초급",
            "intermediate" => "중급",
            "advanced" => "고급",
            _ => Routine.Level ?? ""
        };
    }

    private string GetCategoryText()
    {
        return Routine.Category?.ToLower() switch
        {
            "upper_body" => "상체",
            "lower_body" => "하체",
            "core" => "코어",
            "cardio" => "유산소",
            "full_body" => "전신",
            _ => Routine.Category ?? ""
        };
    }
}