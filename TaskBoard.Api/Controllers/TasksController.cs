using System;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Infrastructure.Persistence;
using TaskBoard.Application.Abstractions;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Dtos.Services;
using TaskBoard.Api.Mappers;
using TaskBoard.Application.Services;
using TaskBoard.Domain;
namespace TaskBoard.Api.Controllers;



[Route("api/[Controller]")]
[ApiController]
public class TasksController : ControllerBase
{

    private readonly ITaskRepository _repo;
    private readonly CreateTask _create;
    private readonly ChangePriority _priority;
    private readonly ChangeStatus _status;

    public TasksController(ITaskRepository repo, CreateTask create, ChangePriority priority, ChangeStatus status)
    {
        
        _repo = repo;
        _create = create;
        _priority = priority;
        _status = status;

    }


    [HttpGet]
    public IActionResult GetAll()
    {
        
        var tasks = _repo.GetAll();

        var dtoList = tasks
            .Select(t => t.ToDto())
            .ToList();

        return Ok(dtoList);

    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        
        var task = _repo.GetById(id);

        if (task is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task '{id}' does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(task.ToDto());

    }


    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskRequest req)
    {
        
        if(string.IsNullOrWhiteSpace(req.Title))
              return BadRequest("Title is required.");
        
        var id = _create.Create(req.ToCommand());

        return Created($"/api/tasks/{id}", new { id });

    }

    [HttpPatch("{id:guid}/priority")]
    public IActionResult ChangePriority(Guid id, [FromBody] ChangePriorityRequest body)
    {
        
        if (!Enum.TryParse<Priority>(body.Priority, ignoreCase: true, out var newPriority))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid priority",
                Detail = $"Unknown priority '{body.Priority}'.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
            {
                _priority.ChangePri(id, newPriority);
                return NoContent(); 
            }
        catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Task not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        

    }

    [HttpPatch("{id:guid}/status")]
        public IActionResult ChangeStatus(Guid id, [FromBody] ChangeStatusRequest body)
    {
        
        if (!Enum.TryParse<Status>(body.Status, ignoreCase: true, out var newStatus))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status",
                Detail = $"Unknown status '{body.Status}'.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            _status.ChangeSta(id, newStatus);
            return NoContent(); 
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Invalid status transition",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }

        }

}
