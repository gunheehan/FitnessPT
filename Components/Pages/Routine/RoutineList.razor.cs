using FitnessPT.Dtos;
using Microsoft.AspNetCore.Components;

namespace FitnessPT.Components.Pages.Routine;

public partial class RoutineList : ComponentBase
{
    private string selectedLevel = "";
    private string selectedCategory = "";
    private bool isLoading = true;
    private bool isEditMode = false;
    private bool showModal = false;
    
    private List<RoutineDto> routineList;
    RoutineDto selectedRoutine = new RoutineDto();

    protected override async Task OnInitializedAsync()
    {
        await LoadExercises();
    }

    private async Task ApplyFilters()
    {
        Console.WriteLine($"필터 적용 - 레벨: {selectedLevel}, 카테고리: {selectedCategory}");
        await LoadExercises();
    }

    private async Task LoadExercises()
    {
        isLoading = true;
        Console.WriteLine($"데이터 로드 중 - 레벨: {selectedLevel}, 카테고리: {selectedCategory}");
        var routines = await Controller.LoadRoutinesAsync(1, 12, selectedLevel, selectedCategory);
        routineList = routines.Items;
        isLoading = false;
    }

    private void OpenCreateModal()
    {
        selectedRoutine = new RoutineDto();
        isEditMode = false;
        showModal = true;
        Console.WriteLine("OnClick Modal Open");
    }

    private void OpenRoutineDetailModal(RoutineDto routine)
    {
        selectedRoutine = routine;
        isEditMode = false;
        showModal = true;
        Console.WriteLine("OnClick Detail Modal Open");
    }

    private async Task SaveRoutine(RoutineInfoDto obj)
    {
        Console.WriteLine("OnClick Save Routine");
        var result = await Controller.CreateRoutineAsync(obj);
        if (result)
        {
            showModal = false;
            StateHasChanged();
        }
    }
}