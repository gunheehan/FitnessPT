using FitnessPT.Dtos;
using FitnessPT.Models;
using FitnessPT.Services;
using Microsoft.JSInterop;

namespace FitnessPT.Components.Controllers;

public class RoutineController
{
    private readonly IRoutineService _routineService;
    private readonly IJSRuntime _js;

    public RoutineController(IRoutineService routineService, IJSRuntime js)
    {
        _routineService = routineService;
        _js = js;
    }
    
    public async Task<PagedResult<RoutineDto>> LoadRoutinesAsync(
        int page, int pageSize, string level = "", string category = "")
    {
        try
        {
            var result = await _routineService.GetRoutineAsync(
                page, pageSize, level, category);

            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading routine: {ex}");
            throw;
        }
    }

    public async Task<RoutineInfoDto> GetRoutineDetail(int id)
    {
        try
        {
            var result = await _routineService.GetRoutineByIdAsync(id);
            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading routine: {ex}");
            throw;
        }
    }

    public async Task<bool> CreateRoutineAsync(RoutineInfoDto routine)
    {
        try
        {
            await _routineService.CreateRoutineAsync(routine);
            await _js.InvokeVoidAsync("alert", "루틴이 추가되었습니다.");
            return true;
        }
        catch (Exception ex)
        {
            await _js.InvokeVoidAsync("alert", $"저장 실패: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateRoutineAsync(int id, RoutineInfoDto routine)
    {
        try
        {
            await _routineService.UpdateRoutineAsync(id, routine);
            await _js.InvokeVoidAsync("alert", "루틴이 수정되었습니다!");
            return true;
        }
        catch (Exception ex)
        {
            await _js.InvokeVoidAsync("alert", $"수정 실패: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteRoutineAsync(int id)
    {
        var confirmed = await _js.InvokeAsync<bool>("confirm", "정말 삭제하시겠습니까?");

        if (!confirmed) return false;

        try
        {
            await _routineService.DeleteRoutineAsync(id);
            await _js.InvokeVoidAsync("alert", "루틴이 삭제되었습니다.");
            return true;
        }
        catch (Exception ex)
        {
            await _js.InvokeVoidAsync("alert", $"삭제 실패: {ex.Message}");
            return false;
        }
    }
}