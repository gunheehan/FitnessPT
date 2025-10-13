using FitnessPT.Dtos;
using FitnessPT.Models;
using FitnessPT.Services.Http;

namespace FitnessPT.Services;

public interface IExerciseService
{
    Task<PagedResult<ExerciseDto>> GetExercisesAsync(
        int page, 
        int pageSize, 
        string level = "", 
        string category = "");
    
    Task<ExerciseDto?> GetExerciseByIdAsync(int id);
    
    Task<ExerciseDto> CreateExerciseAsync(ExerciseDto exercise);
    
    Task UpdateExerciseAsync(int id, ExerciseDto exercise);
    
    Task DeleteExerciseAsync(int id);
}

public class ExerciseService : IExerciseService
{
    private readonly IApiClient apiClient;
    private const string Endpoint = "api/exercises";

    public ExerciseService(IApiClient _apiClient)
    {
        apiClient = _apiClient;
    }

    public async Task<PagedResult<ExerciseDto>> GetExercisesAsync(int page, int pageSize, string level = "", string category = "")
    {
        var queryParams = new Dictionary<string, string>
        {
            { "page", page.ToString() },
            { "pageSize", pageSize.ToString() }
        };

        if (!string.IsNullOrEmpty(level))
            queryParams["level"] = level;

        if (!string.IsNullOrEmpty(category))
            queryParams["category"] = category;

        var response = await apiClient.GetAsync<PagedResult<ExerciseDto>>(Endpoint, queryParams);

        if (response.IsSuccess && response.Data != null)
            return response.Data;

        throw new Exception(response.ErrorMessage ?? "운동 목록 조회 실패");
    }

    public async Task<ExerciseDto> GetExerciseByIdAsync(int id)
    {
        var response = await apiClient.GetByIdAsync<ExerciseDto>(Endpoint, id);

        if (response.IsSuccess && response.Data != null)
            return response.Data;

        throw new Exception(response.ErrorMessage ?? "운동 조회 실패");
    }

    public async Task<ExerciseDto> CreateExerciseAsync(ExerciseDto dto)
    {
        var response = await apiClient.PostAsync<ExerciseDto, ExerciseDto>(Endpoint, dto);

        if (response.IsSuccess && response.Data != null)
            return response.Data;

        throw new Exception(response.ErrorMessage ?? "운동 생성 실패");
    }

    Task IExerciseService.UpdateExerciseAsync(int id, ExerciseDto exercise)
    {
        return UpdateExerciseAsync(id, exercise);
    }

    public async Task<ExerciseDto> UpdateExerciseAsync(int id, ExerciseDto dto)
    {
        var response = await apiClient.PutAsync<ExerciseDto, ExerciseDto>(Endpoint, id, dto);

        if (response.IsSuccess && response.Data != null)
            return response.Data;

        throw new Exception(response.ErrorMessage ?? "운동 수정 실패");
    }

    public async Task DeleteExerciseAsync(int id)
    {
        var response = await apiClient.DeleteAsync(Endpoint, id);

        if (!response.IsSuccess)
            throw new Exception(response.ErrorMessage ?? "운동 삭제 실패");
    }
}