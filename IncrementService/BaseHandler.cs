using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace IncrementService;

public abstract class BaseHandler<TClass, TQuery>
{
    protected readonly ILogger<TClass> _logger;
    protected readonly AbstractValidator<TQuery>? _validator;
    protected readonly string _className;

    protected BaseHandler(ILogger<TClass> logger, AbstractValidator<TQuery>? validator = null)
    {
        _logger = logger;
        _className = nameof(TClass);
        _validator = validator; 
    }

    protected void TraceLogging()
    {
        _logger.LogInformation("Entering Handle method of class {Class}", _className);
    }

    protected async Task ExecValidationsAndThrowOnErrorAsync(TQuery command, CancellationToken token)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            TraceLogging();
        }

        if (_validator is null)
        {
            _logger.LogError("Validator is required and not supplied.");
            throw new InvalidOperationException("Validator is required and not supplied.");
        }

        var validationResult = await _validator.ValidateAsync(command, token).ConfigureAwait(false);

        if (validationResult.IsValid) return;

        string errors = string.Join("; ", validationResult.Errors);
        _logger.LogWarning("In Handler class {ClassName}, validation failed: {Errors}", _className, errors);
        throw new BadRequestException($"Validation failed: {errors}");
    }
}

