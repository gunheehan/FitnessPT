namespace FitnessPT.Dtos;

public class ExerciseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Level { get; set; }
    public string Category { get; set; }
    public string CategoryDetail { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
}