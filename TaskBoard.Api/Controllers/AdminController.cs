using System;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Application.Abstractions;
using TaskBoard.Api.Mappers;

namespace TaskBoard.Api.Controllers;



[Route("api/[Controller]")]
[ApiController]
public sealed class AdminController : ControllerBase
{

    private readonly ITaskRepository _repo;

    public AdminController(ITaskRepository repo) => _repo = repo;

    [HttpGet ("tasks")]
    public IActionResult ExportTasks()
    {
        
        var tasks = _repo.GetAll();

        var dtoList = tasks
            .Select(t => t.ToAdminDto())
            .ToList();

        return Ok(dtoList);

    }

}
