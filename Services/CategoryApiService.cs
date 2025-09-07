namespace FitnessPT.Services;

// Services/CategoryApiService.cs
using System.Text.Json;

public interface ICategoryApiService
{
    Task<List<CategoryDto>?> GetCategoriesAsync(bool includeExercises = false);
    Task<CategoryDto?> GetCategoryByIdAsync(int id, bool includeExercises = false);
}

public class CategoryApiService : ICategoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CategoryApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CategoryApiService(HttpClient httpClient, ILogger<CategoryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null
        };
    }

    public async Task<List<CategoryDto>?> GetCategoriesAsync(bool includeExercises = false)
    {
        try
        {
            var url = $"http://redhorse.iptime.org:6001/api/Categories?includeExercises={includeExercises}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine("####Get Category : " + json);
                return JsonSerializer.Deserialize<List<CategoryDto>>(json, _jsonOptions);
            }

            _logger.LogWarning("카테고리 목록 조회 실패: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 목록 조회 중 오류 발생");
            return null;
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id, bool includeExercises = false)
    {
        try
        {
            var url = $"http://redhorse.iptime.org:6001/api/Categories/{id}?includeExercises={includeExercises}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CategoryDto>(json, _jsonOptions);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "카테고리 상세 조회 중 오류 발생 - ID: {CategoryId}", id);
            return null;
        }
    }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int? ParentCategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryCode { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public List<CategoryDto>? SubCategories { get; set; }
    public List<ExerciseDto>? Exercises { get; set; }
}