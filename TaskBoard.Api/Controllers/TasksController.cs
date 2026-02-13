using System;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Application.Abstractions;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Dtos.Services;
using TaskBoard.Api.Mappers;
using TaskBoard.Application.Services;
using TaskBoard.Api.Helpers;
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
    public ActionResult<PagedResult<TaskDto>> GetByTask([FromQuery] GetTaskQuery q)
    {

        if (q.PageNumber < 1) 
            throw new ArgumentException("Page number must be >= 1.");

        if (q.PageSize < 1 || q.PageSize > 100) 
            throw new ArgumentException("Page size must be 1..100.");

        var page = _repo.GetByTask(q.Priority, q.Status, q.Tags, q.PageNumber, q.PageSize);

        var items = page.Items.Select(t => t.ToDto()).ToList();

        var result = new PagedResult<TaskDto>(items, q.PageNumber, q.PageSize, page.TotalCount);

        return Ok(result);

    }


    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskRequest req)
    {
        
        var id = _create.Create(req.ToCommand());
        var task = _repo.GetById(id)!;

        return CreatedAtAction(nameof(GetById), new { id }, task.ToDto());

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
