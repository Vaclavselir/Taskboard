using System;
using TaskBoard.Application.Abstractions;

namespace TaskBoard.Application.Common;

public class SystemClock : ITime
{

    public DateTime Now => DateTime.Now;

}

public class IdGenerator : IGeneratorId
{
    
    public Guid NewGuid() => Guid.NewGuid();

}
