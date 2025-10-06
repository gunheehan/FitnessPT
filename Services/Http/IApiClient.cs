namespace FitnessPT.Services.Http;

public interface IApiClient
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null);
    Task<ApiResponse<T>> GetByIdAsync<T>(string endpoint, int id);
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, int id, TRequest data);
    Task<ApiResponse<bool>> DeleteAsync(string endpoint, int id);
}