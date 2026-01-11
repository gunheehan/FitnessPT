using FitnessPT.Dtos;
using FitnessPT.Models;
using FitnessPT.Services;
using Microsoft.JSInterop;

namespace FitnessPT.Components.Controllers;

public class ExerciseController
{
    private readonly IExerciseService _exerciseService;
    private readonly IJSRuntime _js;
    
    public ExerciseController(IExerciseService exerciseService, IJSRuntime js)
    {
        _exerciseService = exerciseService;
        _js = js;
    }
    
    public async Task<PagedResult<ExerciseDto>> LoadExercisesAsync(
        int page, 
        int pageSize, 
        string level = "", 
        string category = "")
    {
        try
        {
            var result = await _exerciseService.GetExercisesAsync(
                page: page,
                pageSize: pageSize,
                level: level,
                category: category
            );
            
            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading exercises: {ex}");
            throw;
        }
    }
    
    public async Task<bool> CreateExerciseAsync(ExerciseDto exercise)
    {
        try
        {
            await _exerciseService.CreateExerciseAsync(exercise);
            await _js.InvokeVoidAsync("alert", "운동이 추가되었습니다!");
            return true;
        }
        catch (Exception ex)
        {
            await _js.InvokeVoidAsync("alert", $"저장 실패: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> UpdateExerciseAsync(int id, ExerciseDto exercise)
    {
        try
        {
            await _exerciseService.UpdateExerciseAsync(id, exercise);
            await _js.InvokeVoidAsync("alert", "운동이 수정되었습니다!");
            return true;
        }
        catch (Exception ex)
        {
            await _js.InvokeVoidAsync("alert", $"수정 실패: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> DeleteExerciseAsync(int id)
    {
        var confirmed = await _js.InvokeAsync<bool>("confirm", "정말 삭제하시겠습니까?");
        
        if (!confirmed) return false;
        
        try
        {
            await _exerciseService.DeleteExerciseAsync(id);
            await _js.InvokeVoidAsync("alert", "운동이 삭제되었습니다!");
            return true;
        }
        catch (Exception ex)
        {
            await _js.InvokeVoidAsync("alert", $"삭제 실패: {ex.Message}");
            return false;
        }
    }
}