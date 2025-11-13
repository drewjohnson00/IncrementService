namespace Infrastructure;

public class ErrorResponse
{
    public required int StatusCode { get; set; }
    public required string Message { get; set; }
}
