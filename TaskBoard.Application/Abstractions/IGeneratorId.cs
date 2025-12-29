using System;

namespace TaskBoard.Application.Abstractions;

public interface IGeneratorId
{

    Guid NewGuid();

}
