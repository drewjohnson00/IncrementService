using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class DeleteIncrementCommand : IRequest
{
    public required string Key { get; set; }
}

internal class DeleteIncrementValidator : AbstractValidator<DeleteIncrementCommand>
{
    public DeleteIncrementValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key must be provided.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Key must only contain letters, digits, or underscores.");
    }
}

public class DeleteIncrementHandler : IRequestHandler<DeleteIncrementCommand>
{
    private readonly IIncrementRepository _repository;
    private readonly ILogger<PutIncrementHandler> _logger;
    private readonly string _className = nameof(DeleteIncrementHandler);

    public DeleteIncrementHandler(IIncrementRepository repository, ILogger<PutIncrementHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(DeleteIncrementCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Entering {Method} method of class {Class}", nameof(Handle), _className);

        DeleteIncrementValidator validator = new();
        var validationResult = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            string errors = string.Join("; ", validationResult.Errors);
            _logger.LogWarning("Validation failed for {Method} in class {ClassName}: {Errors}",
                nameof(Handle), _className, errors);
            throw new BadRequestException($"Validation failed: {errors}");
        }

        await _repository.DeleteIncrement(command.Key).ConfigureAwait(false);
    }
}
