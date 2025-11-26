using System;
using System.Collections.Generic;

namespace Common.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
        ValidationErrors = [];
    }

    public BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
        ValidationErrors = [];
    }

    public BadRequestException(string message, List<string> validationErrors) : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public List<string> ValidationErrors { get; }
}
