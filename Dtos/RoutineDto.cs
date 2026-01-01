namespace FitnessPT.Dtos;

public class RoutineDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Level { get; set; }

    public string? Category { get; set; }

    public int? EstimatedDuration { get; set; }

    public string? ThumbnailUrl { get; set; }
}

public class RoutineExerciseDto
{
    public int Id { get; set; }

    public int RoutineId { get; set; }

    public int ExerciseId { get; set; }

    /// <summary>
    /// 루틴 내 운동 순서
    /// </summary>
    public int OrderIndex { get; set; }
    
    /// <summary>
    /// 세트 수
    /// </summary>
    public int? Sets { get; set; }

    /// <summary>
    /// 반복 횟수
    /// </summary>
    public int? Reps { get; set; }

    /// <summary>
    /// 시간 기반 운동(초)
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// 세트 간 휴식(초)
    /// </summary>
    public int? RestSeconds { get; set; }
    
    /// <summary>
    /// 운동 이름
    /// </summary>
    public string ExerciseName { get; set; }
}

public class RoutineInfoDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Level { get; set; }

    public string? Category { get; set; }

    public int? EstimatedDuration { get; set; }

    public List<RoutineExerciseDto> ExerciseInfo { get; set; }
}

public class RoutineUpdateDto
{
    public List<int> DeleteRoutineInfoIDList { get; set; }
    public List<RoutineExerciseDto> AddExerciseInfo { get; set; }
}