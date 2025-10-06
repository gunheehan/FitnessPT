using System.Net.Http.Json;
using System.Text.Json;

namespace FitnessPT.Services.Http;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // GET: api/resource
    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        try
        {
            var url = BuildUrl(endpoint, queryParams);
            _logger.LogInformation("GET Request: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET request failed: {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"요청 실패: {ex.Message}");
        }
    }

    // GET: api/resource/5
    public async Task<ApiResponse<T>> GetByIdAsync<T>(string endpoint, int id)
    {
        try
        {
            var url = $"{endpoint}/{id}";
            _logger.LogInformation("GET Request: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET by ID request failed: {Endpoint}/{Id}", endpoint, id);
            return ApiResponse<T>.Failure($"요청 실패: {ex.Message}");
        }
    }

    // POST: api/resource
    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            _logger.LogInformation("POST Request: {Endpoint}", endpoint);

            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST request failed: {Endpoint}", endpoint);
            return ApiResponse<TResponse>.Failure($"요청 실패: {ex.Message}");
        }
    }

    // PUT: api/resource/5
    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, int id, TRequest data)
    {
        try
        {
            var url = $"{endpoint}/{id}";
            _logger.LogInformation("PUT Request: {Url}", url);

            var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT request failed: {Endpoint}/{Id}", endpoint, id);
            return ApiResponse<TResponse>.Failure($"요청 실패: {ex.Message}");
        }
    }

    // DELETE: api/resource/5
    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, int id)
    {
        try
        {
            var url = $"{endpoint}/{id}";
            _logger.LogInformation("DELETE Request: {Url}", url);

            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.Success(true, (int)response.StatusCode);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Failure(
                $"삭제 실패: {response.StatusCode} - {errorContent}",
                (int)response.StatusCode
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE request failed: {Endpoint}/{Id}", endpoint, id);
            return ApiResponse<bool>.Failure($"요청 실패: {ex.Message}");
        }
    }

    // 응답 처리 헬퍼
    private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            return ApiResponse<T>.Success(data!, (int)response.StatusCode);
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);

        return ApiResponse<T>.Failure(
            $"API 오류: {response.StatusCode} - {errorContent}",
            (int)response.StatusCode
        );
    }

    // URL 빌더
    private string BuildUrl(string endpoint, Dictionary<string, string>? queryParams)
    {
        if (queryParams == null || !queryParams.Any())
            return endpoint;

        var query = string.Join("&", queryParams
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        return $"{endpoint}?{query}";
    }
}