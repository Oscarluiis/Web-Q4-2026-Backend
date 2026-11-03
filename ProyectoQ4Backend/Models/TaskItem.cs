namespace ProyectoQ4Backend.Models;

public class TaskItem
{
  public string Id { get; set; } = string.Empty;
  
  public string Title { get; set; } = string.Empty;
  
  public string Description { get; set; } = string.Empty;

  public string Priority { get; set; } = "media";
  
  public string UserId { get; set; } = string.Empty;
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}