using Microsoft.JSInterop.Infrastructure;
using ProyectoQ4Backend.DTOs;
using ProyectoQ4Backend.Models;

namespace ProyectoQ4Backend.Services;

public class TaskService
{
    private readonly FirebaseService _firebaseService;

    // Inyeccion de dependencias
    public TaskService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<TaskItem> Create(TaskDto dto, string userId)
    {
        var task = new TaskItem()
        {
            Id = Guid.NewGuid().ToString(),
            Title = dto.Title,
            Description =  dto.Description,
            Priority =  dto.Priority,
            UserId = userId,
            CreatedAt =  DateTime.UtcNow
        };

        await _firebaseService.GetCollection("task")
            .Document(task.Id)
            .SetAsync(new Dictionary<string, object>
            {
                { "Id",  task.Id },
                { "Title", task.Title },
                { "Description", task.Description },
                { "Priority", task.Priority },
                { "UserId", task.UserId },
                { "CreatedAt", task.CreatedAt }
            });
        return task;
    }

    public async Task<List<TaskItem>> GetByUser(string userId)
    {
        var snapshot = await _firebaseService.GetCollection("task")
            .WhereEqualTo("UserId", userId)
            .GetSnapshotAsync();
        
        var task = new List<TaskItem>();

        foreach (var doc in snapshot.Documents)
        {
            var data = doc.ToDictionary();
            task.Add(new TaskItem
            {
                Id = data["Id"].ToString()!,
                Title = data["Title"].ToString()!,
                Description = data["Description"].ToString()!,
                Priority = data["Priority"].ToString()!,
                UserId = data["UserId"].ToString()!,
                CreatedAt = ((Google.Cloud.Firestore.Timestamp)data["CreatedAt"]).ToDateTime()
            });
        }
        return task;
    }
}