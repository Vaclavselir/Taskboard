using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Application.Abstractions;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Dtos.Services;
using TaskBoard.Api.Mappers;
using TaskBoard.Application.Services;
using TaskBoard.Api.Helpers;
using TaskBoard.Domain;
using System.Security.Claims;
namespace TaskBoard.Api.Controllers;


[Route("api/[Controller]")]
[ApiController]
[Authorize]
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

    [HttpGet("mine")]
    public IActionResult GetMine()
    {
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult GetAdminStuff()
    {

        return Ok();

    }

    [HttpGet("{id:guid}")]
    public ActionResult<TaskDto> GetById([FromRoute] Guid id)
    {
        
        var ownerId = GetCurrentOwnerId();

        var task = _repo.GetById(ownerId, id);

        if (task is null)
        {
            return NotFound(new ProblemDetails
            {

                Title = "Task not found",
                Detail = $"Task '{id}' does not exist.",
                Status = 404

            });
        }

        TaskDto dto = task.ToDto();

        Response.Headers.ETag = $"\"{dto.RowVersion}\"";

        return Ok(dto);

    }

    [HttpGet]
    public ActionResult<PagedResult<TaskDto>> GetByTask([FromQuery] GetTaskQuery q)
    {

        var ownerId = GetCurrentOwnerId();

        if (q.PageNumber < 1) 
            throw new ArgumentException("Page number must be >= 1.");

        if (q.PageSize < 1 || q.PageSize > 100) 
            throw new ArgumentException("Page size must be 1..100.");

        var page = _repo.GetByTask(ownerId, q.Priority, q.Status, q.Tags, q.PageNumber, q.PageSize);

        var items = page.Items.Select(t => t.ToDto()).ToList();

        var result = new PagedResult<TaskDto>(items, q.PageNumber, q.PageSize, page.TotalCount);

        return Ok(result);

    }


    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskRequest req)
    {
        
        var ownerId = GetCurrentOwnerId();

        var id = _create.Create(ownerId, req.ToCommand());
        var task = _repo.GetById(ownerId, id)!;

        return CreatedAtAction(nameof(GetById), new { id }, task.ToDto());

    }

    [HttpPatch("{id:guid}")]
    public IActionResult Patch(Guid id, [FromHeader(Name = "If-Match")] string? ifMatch, [FromBody] UpdateTaskRequest body)
    {

        var ownerId = GetCurrentOwnerId();

        TaskItem? task = _repo.GetById(ownerId, id);      
        if (task is null) return NotFound();

        if (!Matches(ifMatch, task.RowVersion))
            return Conflict(new { message = "Task byl mezitím změněn. Udělej GET a zkus to znovu." });

        var changed = _update.Update(
            ownerId,
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
        
        var ownerId = GetCurrentOwnerId();

        _delete.Delete(ownerId, id);
            
        return NoContent(); 
  
    }

    private static bool Matches(string? ifMatch, byte[] current)
    {

        if (string.IsNullOrWhiteSpace(ifMatch))
            return true; 

        var token = ifMatch.Trim();

        
        if (token.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
            token = token[2..].Trim();

        token = token.Trim('"');

        var currentToken = Convert.ToBase64String(current);

        return string.Equals(token, currentToken, StringComparison.Ordinal);

    }

    private string GetCurrentOwnerId()
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new UnauthorizedAccessException("Authenticated user id was not found.");

        return ownerId;
    }

}
