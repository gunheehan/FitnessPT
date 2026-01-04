using FitnessPT.Dtos;
using FitnessPT.Models;
using FitnessPT.Services.Http;

namespace FitnessPT.Services;

public interface IRoutineService
{
    Task<PagedResult<RoutineDto>> GetRoutineAsync(
        int page,
        int pageSize,
        string level = "",
        string category = "");

    Task<RoutineInfoDto> CreateRoutineAsync(RoutineInfoDto routine);
    Task<RoutineInfoDto> UpdateRoutineAsync(int id, RoutineInfoDto routine);
    Task DeleteRoutineAsync(int id);
}

public class RoutineService : IRoutineService
{
    private readonly IApiClient apiClient;
    private const string Endpoint = "/api/routine";

    public RoutineService(IApiClient _apiClient)
    {
        apiClient = _apiClient;
    }
    public async Task<PagedResult<RoutineDto>> GetRoutineAsync(int page, int pageSize, string level = "", string category = "")
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

        var response = await apiClient.GetAsync<PagedResult<RoutineDto>>(Endpoint, queryParams);

        if (response.IsSuccess && response.Data != null)
            return response.Data;

        throw new Exception(response.ErrorMessage ?? "루틴 목록 조회 실패");
    }

    public async Task<RoutineInfoDto> CreateRoutineAsync(RoutineInfoDto routine)
    {
        var response = await apiClient.PostAsync<RoutineInfoDto, RoutineInfoDto>(Endpoint, routine);

        if (response.IsSuccess && response.Data != null)
            return response.Data;

        throw new Exception(response.ErrorMessage ?? "루틴 생성 실패");
    }
    
    public async Task<RoutineInfoDto> UpdateRoutineAsync(int id, RoutineInfoDto dto)
    {
        var response = await apiClient.PutAsync<RoutineInfoDto, RoutineInfoDto>(Endpoint, id, dto);

        if (response.IsSuccess && response.Data != null)
            return response.Data;

        throw new Exception(response.ErrorMessage ?? "루틴 수정 실패");
    }

    public async Task DeleteRoutineAsync(int id)
    {
        var response = await apiClient.DeleteAsync(Endpoint, id);

        if (!response.IsSuccess)
            throw new Exception(response.ErrorMessage ?? "루틴 삭제 실패");
    }
}