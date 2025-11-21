using System;
using System.Linq;
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

    protected BaseHandler(ILogger<TClass> logger, AbstractValidator<TQuery>? validator = null)
    {
        _logger = logger;
        _validator = validator; 
    }

    protected void TraceLogging()
    {
        _logger.LogInformation(""); // Since we have source context, this is sufficient for trace logging...
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

        throw new BadRequestException("Validation failed", validationResult.Errors.Select(x => x.ErrorMessage).ToList());   
    }
}

