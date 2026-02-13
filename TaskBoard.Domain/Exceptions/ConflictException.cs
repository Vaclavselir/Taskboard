using System;

namespace TaskBoard.Domain.Exceptions;

public sealed class ConflictException : Exception
{

    public ConflictException(string message) : base(message){}

}
