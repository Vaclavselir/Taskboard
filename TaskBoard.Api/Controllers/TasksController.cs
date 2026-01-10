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
    private readonly DeleteTask _delete;

    public TasksController(ITaskRepository repo, CreateTask create, ChangePriority priority, ChangeStatus status, DeleteTask delete)
    {
        
        _repo = repo;
        _create = create;
        _priority = priority;
        _status = status;
        _delete = delete;

    }

    /*
    [HttpGet]
    public IActionResult GetAll()
    {
        
        var tasks = _repo.GetAll();

        var dtoList = tasks
            .Select(t => t.ToDto())
            .ToList();

        return Ok(dtoList);

    }
    */

    [HttpGet("{id:guid}")]
    public ActionResult<IEnumerable<TaskDto>> GetById([FromRoute] Guid id)
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

    [HttpGet]
    public  ActionResult<IEnumerable<TaskDto>> GetByTask([FromQuery] Priority? priority, [FromQuery] Status? status, [FromQuery] List<string>? tags)
    {
        
        if (priority is not null &&  !Enum.IsDefined<Priority>(priority.Value))
        {

            return BadRequest(new ProblemDetails
            {

                Title = "Invalid priority",

                Detail = $"Unknown priority '{priority}'.",

                Status = StatusCodes.Status400BadRequest

            });

        }

        if (status is not null &&  !Enum.IsDefined<Status>(status.Value))
        {

            return BadRequest(new ProblemDetails
            {

                Title = "Invalid status",

                Detail = $"Unknown status '{status}'.",

                Status = StatusCodes.Status400BadRequest
                
            });

        }

        tags = tags?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (tags is { Count: 0 })
            tags = null;


        var tasks = _repo.GetByTask(priority, status, tags);

        var result = tasks.Select(t => t.ToDto()).ToList();

        return Ok(result);

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

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        
        try
        {
            _delete.Delete(id);
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

}
