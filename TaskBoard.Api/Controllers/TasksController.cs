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
    private readonly Updatetask _update;
    private readonly DeleteTask _delete;

    public TasksController(ITaskRepository repo, CreateTask create, Updatetask update, DeleteTask delete)
    {
        
        _repo = repo;
        _create = create;
        _update = update;
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
    public ActionResult<TaskDto> GetById([FromRoute] Guid id)
    {
        
        var task = _repo.GetById(id);

        if (task is null)
            return NotFound(new ProblemDetails
            {

                Title = "Task not found",
                Detail = $"Task '{id}' does not exist.",
                Status = 404

            });

        return Ok(task.ToDto());

    }

    [HttpGet]
    public ActionResult<IEnumerable<TaskDto>> GetByTask([FromQuery] Priority? priority, [FromQuery]  Status? status, [FromQuery]  List<string>? tags)
    {

        var tasks = _repo.GetByTask(priority, status, tags);

        var result = tasks.Select(t => t.ToDto()).ToList();

        return Ok(result);

    }


    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskRequest req)
    {
        
        var id = _create.Create(req.ToCommand());

        return Created($"/api/tasks/{id}", new { id });

    }

    [HttpPatch("{id:guid}")]
    public IActionResult Patch(Guid id, [FromBody] UpdateTaskRequest body)
    {

        var changed = _update.Update(
            id,
            newTitle: body.Title, 
            newDescription: body.Description,
            newDueDate: body.DueDate, 
            newStatus: body.Status, 
            newPriority: body.Priority
        );

    
        return NoContent();

    }


    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        
            _delete.Delete(id);
            
            return NoContent(); 
  
    }

}
