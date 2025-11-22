using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Exceptions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IncrementService.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        Guid logId = Guid.NewGuid();
        HttpStatusCode statusCode;
        string message;
        List<string>? errors = null;

        switch (exception)
        {
            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundException.Message;
                _logger.LogWarning(notFoundException, "Resource not found: {Message}. LogId: {LogId}", message, logId);
                break;

            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                message = badRequestException.Message;
                errors = badRequestException.ValidationErrors;
                _logger.LogWarning(badRequestException, "Bad request: {Message}. LogId: {LogId}", message, logId);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An internal server error occurred.";
                _logger.LogError(exception, "An unhandled exception occurred: {Message}. LogId: {LogId}", exception.Message, logId);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        context.Response.Headers["X-Correlation-Id"] = logId.ToString();

        var response = new ErrorResponse
        {
            Message = message,
            ValidationErrors = errors
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
