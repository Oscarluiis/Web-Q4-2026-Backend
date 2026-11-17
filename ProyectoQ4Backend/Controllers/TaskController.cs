using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoQ4Backend.DTOs;
using ProyectoQ4Backend.Services;

namespace ProyectoQ4Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;
    
    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }

    // api/Task
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var task = await _taskService.Create(dto, userId);
            return Ok(task);
        }
        catch (Exception e)
        {
            return BadRequest(new {message = e.Message});
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var task = await _taskService.GetByUser(userId);
            
            return Ok(task);

        }
        catch (Exception e)
        {
            return BadRequest(new {message = e.Message});
        }
    }
}