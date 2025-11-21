using System.Collections.Generic;

namespace Infrastructure;

public class ErrorResponse
{
    public required string Message { get; set; }

    public List<string>? Errors { get; set; }
}
