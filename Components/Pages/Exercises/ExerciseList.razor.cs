using FitnessPT.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if(!AuthService.IsAuthenticated)
                Navigation.NavigateTo("/", forceLoad: true);
        }
    }

    private async Task LoadExercises()
    {
        isLoading = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var result = await ExerciseService.GetExercisesAsync(
                page: currentPage,
                pageSize: pageSize,
                level: selectedLevel,
                category: selectedCategory
            );

            exercises = result.Items;
            totalCount = result.TotalCount;
            totalPages = result.TotalPages;
        }
        catch (HttpRequestException ex)
        {
            errorMessage = $"서버 연결 실패: {ex.Message}";
            Console.Error.WriteLine($"API Error: {ex}");
        }
        catch (Exception ex)
        {
            errorMessage = $"오류 발생: {ex.Message}";
            Console.Error.WriteLine($"Error: {ex}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    #region Modal

    private void OpenAuthModal()
    {
        showAuth = false;
        isAuth = true;
    }

    private void CloseAuthModal()
    {
        showAuth = false;
    }
    
    private void OpenCreateModal()
    {
        selectedExercise = new ExerciseDto();
        isEditMode = false;
        showModal = true;

        if (!isAuth)
            showAuth = true;
    }

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
    
    private async Task SaveExercise(ExerciseDto exercise)
    {
        try
        {
            if (isEditMode)
            {
                var updateDto = new ExerciseDto
                {
                    Name = exercise.Name,
                    Description = exercise.Description,
                    Level = exercise.Level,
                    Category = exercise.Category,
                    CategoryDetail = exercise.CategoryDetail,
                    ImageUrl = exercise.ImageUrl,
                    VideoUrl = exercise.VideoUrl
                };

                await ExerciseService.UpdateExerciseAsync(exercise.Id, updateDto);
                await JS.InvokeVoidAsync("alert", "운동이 수정되었습니다!");
            }
            else
            {
                var createDto = new ExerciseDto
                {
                    Name = exercise.Name,
                    Description = exercise.Description,
                    Level = exercise.Level,
                    Category = exercise.Category,
                    CategoryDetail = exercise.CategoryDetail,
                    ImageUrl = exercise.ImageUrl,
                    VideoUrl = exercise.VideoUrl
                };

                await ExerciseService.CreateExerciseAsync(createDto);
                await JS.InvokeVoidAsync("alert", "운동이 추가되었습니다!");
            }

            CloseEditModal();
            await LoadExercises();
        }
        catch (HttpRequestException ex)
        {
            await JS.InvokeVoidAsync("alert", $"서버 연결 실패: {ex.Message}");
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"저장 실패: {ex.Message}");
        }
    }

    private void CloseEditModal()
    {
        showModal = false;
        selectedExercise = new();
    }

    #endregion
    
    private async Task SearchExercises()
    {
        currentPage = 1;
        await LoadExercises();
    }

    private async Task ApplyFilters()
    {
        currentPage = 1;
        await LoadExercises();
    }

    private async Task ClearFilters()
    {
        searchTerm = "";
        selectedLevel = "";
        selectedCategory = "";
        currentPage = 1;
        await LoadExercises();
    }

    private async Task ChangePage(int page)
    {
        currentPage = page;
        await LoadExercises();
        
        // 페이지 상단으로 스크롤
        await JS.InvokeVoidAsync("window.scrollTo", 0, 0);
    }

    private async Task DeleteExercise(int id)
    {
        if (!isAuth)
        {
            showAuth = true;
            return;
        }
        var confirmed = await JS.InvokeAsync<bool>("confirm", "정말 삭제하시겠습니까?");
        
        if (confirmed)
        {
            try
            {
                await ExerciseService.DeleteExerciseAsync(id);
                await JS.InvokeVoidAsync("alert", "운동이 삭제되었습니다!");
                
                // 현재 페이지에 아이템이 없으면 이전 페이지로
                if (exercises?.Count == 1 && currentPage > 1)
                {
                    currentPage--;
                }
                
                await LoadExercises();
            }
            catch (HttpRequestException ex)
            {
                await JS.InvokeVoidAsync("alert", $"서버 연결 실패: {ex.Message}");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("alert", $"삭제 실패: {ex.Message}");
            }
        }
    }
}