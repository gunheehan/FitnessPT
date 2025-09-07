namespace FitnessPT.Services;

// Services/ExerciseApiService.cs
using System.Text;
using System.Text.Json;

public interface IExerciseApiService
{
    Task<ApiResponse<List<ExerciseDto>>> GetExercisesAsync(int page = 1, int pageSize = 20,
        string? searchTerm = null, int? categoryId = null, int? difficultyLevel = null, bool? isActive = null);
    Task<ApiResponse<bool>> UpdateExerciseStatusAsync(int exerciseId, bool isActive);

    Task<ExerciseDto?> GetExerciseByIdAsync(int id);
    Task<ExerciseDto?> CreateExerciseAsync(CreateExerciseDto exerciseData);
    Task<ExerciseDto?> UpdateExerciseAsync(int id, UpdateExerciseDto exerciseData);
    Task<bool> DeleteExerciseAsync(int id);
    Task<bool> ToggleExerciseStatusAsync(int id);
}

public class ExerciseApiService : IExerciseApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExerciseApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExerciseApiService(HttpClient httpClient, ILogger<ExerciseApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null
        };
    }

    public async Task<ApiResponse<List<ExerciseDto>>> GetExercisesAsync(int page = 1, int pageSize = 20,
        string? searchTerm = null, int? categoryId = null, int? difficultyLevel = null, bool? isActive = null)
    {
        try
        {
            // Circuit이 끊어진 상태에서 호출되지 않도록 체크
            if (_httpClient == null)
                return new ApiResponse<List<ExerciseDto>>();
            
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrEmpty(searchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            if (categoryId.HasValue)
                queryParams.Add($"categoryId={categoryId}");
            if (difficultyLevel.HasValue)
                queryParams.Add($"difficultyLevel={difficultyLevel}");
            if (isActive.HasValue)
                queryParams.Add($"isActive={isActive}");

            var url = $"http://redhorse.iptime.org:6001/api/Exercises?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine("####Get Exercise : " + json);

                var exercises = JsonSerializer.Deserialize<List<ExerciseDto>>(json, _jsonOptions) ??
                                new List<ExerciseDto>();

                var totalCount = GetHeaderValue(response, "X-Total-Count");
                var totalPages = GetHeaderValue(response, "X-Total-Pages");

                return new ApiResponse<List<ExerciseDto>>
                {
                    Success = true,
                    Data = exercises,
                    TotalCount = int.TryParse(totalCount, out var count) ? count : 0,
                    TotalPages = int.TryParse(totalPages, out var pages) ? pages : 1
                };
            }

            return new ApiResponse<List<ExerciseDto>> { Success = false, ErrorMessage = "데이터 조회 실패" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 목록 조회 중 오류 발생");
            return new ApiResponse<List<ExerciseDto>> { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> UpdateExerciseStatusAsync(int exerciseId, bool isActive)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
            var updateData = new { IsActive = isActive };
            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
        
            var response = await _httpClient.PatchAsync($"api/exercises/{exerciseId}/status", content, cts.Token);
        
            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    ErrorMessage = $"상태 변경 실패: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 상태 변경 중 오류 발생");
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ExerciseDto?> GetExerciseByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://redhorse.iptime.org:6001/api/Exercises/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExerciseDto>(json, _jsonOptions);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 상세 조회 중 오류 발생 - ID: {ExerciseId}", id);
            return null;
        }
    }

    public async Task<ExerciseDto?> CreateExerciseAsync(CreateExerciseDto exerciseData)
    {
        try
        {
            var json = JsonSerializer.Serialize(exerciseData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://redhorse.iptime.org:6001/api/Exercises", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExerciseDto>(responseJson, _jsonOptions);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 생성 중 오류 발생");
            return null;
        }
    }

    public async Task<ExerciseDto?> UpdateExerciseAsync(int id, UpdateExerciseDto exerciseData)
    {
        try
        {
            var json = JsonSerializer.Serialize(exerciseData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"http://redhorse.iptime.org:6001/api/Exercises/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExerciseDto>(responseJson, _jsonOptions);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 수정 중 오류 발생 - ID: {ExerciseId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteExerciseAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"http://redhorse.iptime.org:6001/api/Exercises/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 삭제 중 오류 발생 - ID: {ExerciseId}", id);
            return false;
        }
    }

    public async Task<bool> ToggleExerciseStatusAsync(int id)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"http://redhorse.iptime.org:6001/api/Exercises/{id}/toggle-status", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "운동 상태 변경 중 오류 발생 - ID: {ExerciseId}", id);
            return false;
        }
    }

    private string GetHeaderValue(HttpResponseMessage response, string headerName)
    {
        return response.Headers.TryGetValues(headerName, out var values) ? values.FirstOrDefault() ?? "" : "";
    }
}

// Models
public class ExerciseDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = null!;
    public int? PrimaryCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryCode { get; set; }
    public int? DifficultyLevel { get; set; }
    public string? DifficultyName { get; set; }
    public string? TargetMuscles { get; set; }
    public string? Instructions { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateExerciseDto
{
    public string ExerciseName { get; set; } = null!;
    public int? PrimaryCategoryId { get; set; }
    public int? DifficultyLevel { get; set; }
    public string? TargetMuscles { get; set; }
    public string? Instructions { get; set; }
}

public class UpdateExerciseDto
{
    public string? ExerciseName { get; set; }
    public int? PrimaryCategoryId { get; set; }
    public int? DifficultyLevel { get; set; }
    public string? TargetMuscles { get; set; }
    public string? Instructions { get; set; }
    public bool? IsActive { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}