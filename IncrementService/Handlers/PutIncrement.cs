using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Common.Exceptions;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class PutIncrementCommand : IncrementCommand, IRequest<IncrementKey>;

public class PutIncrementHandler : IRequestHandler<PutIncrementCommand, IncrementKey>
{
    private readonly IIncrementRepository _repository;
    private readonly ILogger<PutIncrementHandler> _logger;
    private readonly string _className = nameof(PutIncrementHandler);

    public PutIncrementHandler(IIncrementRepository repository, ILogger<PutIncrementHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    internal class PutIncrementValidator : AbstractValidator<PutIncrementCommand>
    {
        public PutIncrementValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Key must be provided.")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Key must only contain letters, digits, or underscores.");
            RuleFor(x => x.PreviousValue)
                .Must(previousValue => previousValue.HasValue is false || previousValue.Value >= 0)
                .WithMessage("PreviousValue must be greater than or equal to 0.");
        }
    }

    public async Task<IncrementKey> Handle(PutIncrementCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Entering {nameof(Handle)} method of class {_className}");

        PutIncrementValidator validator = new();
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (validationResult.IsValid is false)
        {
            string errors = string.Join("; ", validationResult.Errors);
            _logger.LogWarning("Validation failed for {Method} in class {ClassName}: {Errors}",
                nameof(Handle), _className, errors);
            throw new BadRequestException($"Validation failed: {errors}");
        }

        IncrementKey result = await _repository.UpsertIncrement(request).ConfigureAwait(false);
        return result;
    }
}
