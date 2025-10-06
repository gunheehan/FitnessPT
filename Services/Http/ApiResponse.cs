namespace FitnessPT.Services.Http;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Success(T data, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Failure(string errorMessage, int statusCode = 500)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}