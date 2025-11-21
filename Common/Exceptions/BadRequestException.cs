using System;
using System.Collections.Generic;

namespace Common.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
        Errors = [];
    }

    public BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = [];
    }

    public BadRequestException(string message, List<string> errors) : base(message)
    {
        Errors = errors;
    }

    public List<string> Errors { get; }
}
