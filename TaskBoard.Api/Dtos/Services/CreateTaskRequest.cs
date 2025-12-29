using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Services;
using TaskBoard.Domain;
using TaskBoard.Infrastructure.Persistence;

namespace TaskBoard.Api.Dtos.Services;

public sealed record CreateTaskRequest
(

    string Title,

    string? Description,

    Priority Priority,

    DateTime? DueDate,

    IEnumerable<string>? Tags
    
);