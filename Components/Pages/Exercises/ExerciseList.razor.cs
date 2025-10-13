using FitnessPT.Dtos;
using Microsoft.AspNetCore.Components;

namespace FitnessPT.Components.Pages.Exercises;

public partial class ExerciseList : ComponentBase
{
    private List<ExerciseDto>? exercises;
    private ExerciseDto selectedExercise = new();

    private string searchTerm = "";
    private string selectedLevel = "";
    private string selectedCategory = "";

    private bool isLoading = false;
    private bool showModal = false;
    private bool showAuth = false;
    private bool isAuth = false;
    private bool isEditMode = false;
    private string? errorMessage = null;

    private int currentPage = 1;
    private int pageSize = 12;
    private int totalPages = 1;
    private int totalCount = 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadExercises();
    }

    private async Task LoadExercises()
    {
        isLoading = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var result = await Controller.LoadExercisesAsync(
                currentPage,
                pageSize,
                selectedLevel,
                selectedCategory
            );

            exercises = result.Items;
            totalCount = result.TotalCount;
            totalPages = result.TotalPages;
        }
        catch (Exception ex)
        {
            errorMessage = $"오류 발생: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    // 필터 변경
    private async Task ApplyFilters()
    {
        currentPage = 1;
        await LoadExercises();
    }

    // 필터 초기화
    private async Task ClearFilters()
    {
        searchTerm = "";
        selectedLevel = "";
        selectedCategory = "";
        currentPage = 1;
        await LoadExercises();
    }

    // 페이지 전환
    private async Task ChangePage(int page)
    {
        currentPage = page;
        await LoadExercises();
    }

    private async Task SaveExercise(ExerciseDto exercise)
    {
        bool success = isEditMode
            ? await Controller.UpdateExerciseAsync(exercise.Id, exercise)
            : await Controller.CreateExerciseAsync(exercise);

        if (success)
        {
            CloseEditModal();
            await LoadExercises();
        }
    }

    private async Task DeleteExercise(int id)
    {
        if (!isAuth)
        {
            showAuth = true;
            return;
        }

        bool success = await Controller.DeleteExerciseAsync(id);

        if (success)
        {
            if (exercises?.Count == 1 && currentPage > 1)
            {
                currentPage--;
            }

            await LoadExercises();
        }
    }

    #region Modal 관련 메서드

    // 관리자 인증 성공
    private void OpenAuthModal()
    {
        isAuth = true;
        showAuth = false;
    }

    // 관리자 인증 모달 닫기
    private void CloseAuthModal()
    {
        showAuth = false;
    }

    // 생성 모달 열기
    private void OpenCreateModal()
    {
        selectedExercise = new ExerciseDto();
        isEditMode = false;
        showModal = true;

        if (!isAuth)
            showAuth = true;
    }

    // 수정 모달 열기
    private void OpenEditModal(ExerciseDto exercise)
    {
        selectedExercise = new ExerciseDto
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Level = exercise.Level,
            Category = exercise.Category,
            CategoryDetail = exercise.CategoryDetail,
            ImageUrl = exercise.ImageUrl,
            VideoUrl = exercise.VideoUrl
        };
        isEditMode = true;
        showModal = true;

        if (!isAuth)
            showAuth = true;
    }

    // 수정 모달 닫기
    private void CloseEditModal()
    {
        showModal = false;
        selectedExercise = new();
    }

    #endregion
}