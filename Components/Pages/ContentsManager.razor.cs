// using FitnessPT.Modals.ExerciseModals;
// using FitnessPT.Services;
// using Microsoft.AspNetCore.Components;
// using Microsoft.AspNetCore.Components.Web;
// using Microsoft.JSInterop;
//
// namespace FitnessPT.Components.Pages;
//
// public partial class ContentsManager : ComponentBase
// {
//     
//     // 필드
//     private List<ExerciseDto>? exercises;
//     private List<CategoryDto>? categories;
//     private bool isLoading = true;
//     private string searchTerm = "";
//     private string selectedCategoryId = "";
//     private string selectedDifficultyLevel = "";
//     private string selectedIsActive = "";
//     private int currentPage = 1;
//     private int pageSize = 12;
//     private int totalCount = 0;
//     private int totalPages = 0;
//
//     // 모달 참조
//     private CreateExerciseModal createModal = null!;
//     private EditExerciseModal editModal = null!;
//     private DeleteExerciseModal deleteModal = null!;
//     private ExerciseDetailsModal detailsModal = null!;
//
//     protected override async Task OnInitializedAsync()
//     {
//         await LoadCategories();
//         await LoadExercises();
//     }
//
//     private async Task LoadCategories()
//     {
//         try
//         {
//             categories = await CategoryApiService.GetCategoriesAsync();
//         }
//         catch (Exception ex)
//         {
//             Logger.LogError(ex, "카테고리 로드 중 오류 발생");
//         }
//     }
//
//     private async Task LoadExercises()
//     {
//         isLoading = true;
//         StateHasChanged();
//
//         try
//         {
//             int? categoryId = string.IsNullOrEmpty(selectedCategoryId) ? null : int.Parse(selectedCategoryId);
//             int? difficultyLevel = string.IsNullOrEmpty(selectedDifficultyLevel) ? null : int.Parse(selectedDifficultyLevel);
//             bool? isActive = string.IsNullOrEmpty(selectedIsActive) ? null : bool.Parse(selectedIsActive);
//
//             var result = await ExerciseApiService.GetExercisesAsync(currentPage, pageSize, 
//                 string.IsNullOrEmpty(searchTerm) ? null : searchTerm, categoryId, difficultyLevel, isActive);
//
//             if (result.Success)
//             {
//                 exercises = result.Data;
//                 totalCount = result.TotalCount;
//                 totalPages = result.TotalPages;
//             }
//             else
//             {
//                 exercises = new List<ExerciseDto>();
//                 //await JSRuntime.InvokeVoidAsync("alert", $"데이터 로드 실패: {result.ErrorMessage}");
//             }
//         }
//         catch (Exception ex)
//         {
//             Logger.LogError(ex, "운동 목록 로드 중 오류 발생");
//             //await JSRuntime.InvokeVoidAsync("alert", "데이터를 불러오는 중 오류가 발생했습니다.");
//         }
//         finally
//         {
//             isLoading = false;
//             StateHasChanged();
//         }
//     }
//
//     private async Task OnSearchKeyPress(KeyboardEventArgs e)
//     {
//         if (e.Key == "Enter")
//         {
//             currentPage = 1;
//             await LoadExercises();
//         }
//     }
//
//     private async Task ChangePage(int page)
//     {
//         currentPage = page;
//         await LoadExercises();
//     }
//
//     private async Task ClearFilters()
//     {
//         searchTerm = "";
//         selectedCategoryId = "";
//         selectedDifficultyLevel = "";
//         selectedIsActive = "";
//         currentPage = 1;
//         await LoadExercises();
//     }
//
//     private async Task ToggleExerciseStatus(int exerciseId, bool newStatus)
//     {
//         try
//         {
//             var result = await ExerciseApiService.ToggleExerciseStatusAsync(exerciseId);
//             if (result)
//             {
//                 var exercise = exercises?.FirstOrDefault(e => e.ExerciseId == exerciseId);
//                 if (exercise != null)
//                 {
//                     exercise.IsActive = newStatus;
//                     StateHasChanged();
//                 }
//             }
//             else
//             {
//                 await JSRuntime.InvokeVoidAsync("alert", "상태 변경에 실패했습니다.");
//             }
//         }
//         catch (Exception ex)
//         {
//             Logger.LogError(ex, "운동 상태 변경 중 오류 발생");
//             await JSRuntime.InvokeVoidAsync("alert", "상태 변경 중 오류가 발생했습니다.");
//         }
//     }
//
//     // 모달 관련 메서드
//     private async Task ShowCreateModal()
//     {
//         await createModal.ShowAsync();
//     }
//
//     private async Task ShowEditModal(ExerciseDto exercise)
//     {
//         await editModal.ShowAsync(exercise);
//     }
//
//     private async Task ShowDeleteModal(ExerciseDto exercise)
//     {
//         await deleteModal.ShowAsync(exercise);
//     }
//
//     private async Task ShowDetailsModal(ExerciseDto exercise)
//     {
//         await detailsModal.ShowAsync(exercise);
//     }
//
//     // 이벤트 핸들러
//     private async Task OnExerciseCreated()
//     {
//         await LoadExercises();
//         await JSRuntime.InvokeVoidAsync("alert", "운동이 성공적으로 추가되었습니다.");
//     }
//
//     private async Task OnExerciseUpdated()
//     {
//         await LoadExercises();
//         await JSRuntime.InvokeVoidAsync("alert", "운동이 성공적으로 수정되었습니다.");
//     }
//
//     private async Task OnExerciseDeleted()
//     {
//         await LoadExercises();
//         await JSRuntime.InvokeVoidAsync("alert", "운동이 성공적으로 삭제되었습니다.");
//     }
//
//     // 헬퍼 메서드
//     private string GetDifficultyBadgeClass(int? difficultyLevel)
//     {
//         return difficultyLevel switch
//         {
//             1 => "bg-success",
//             2 => "bg-warning",
//             3 => "bg-danger",
//             4 => "bg-dark",
//             _ => "bg-secondary"
//         };
//     }
// }