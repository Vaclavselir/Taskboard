using System;
using TaskBoard.Api.Dtos.Services;
using TaskBoard.Application.Services;

namespace TaskBoard.Api.Mappers;

public static class CreateTaskMapper
{

    public static TaskCommand ToCommand (this CreateTaskRequest req) =>
        new(

            req.Title,
        
            req.Description,
         
            req.Priority,
          
            req.DueDate,
           
            req.Tags

        );


}
