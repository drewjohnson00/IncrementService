using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Common.Exceptions;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class GetIncrementQuery : IRequest<IncrementKey>
{
    public required string Key { get; set; }
}

public class GetIncrementHandler : IRequestHandler<GetIncrementQuery, IncrementKey>
{
    private readonly IIncrementRepository _repository;
    private readonly ILogger<GetIncrementHandler> _logger;
    private readonly string _className = nameof(GetIncrementHandler);

    public GetIncrementHandler(IIncrementRepository repository, ILogger<GetIncrementHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    internal class GetIncrementValidator : AbstractValidator<GetIncrementQuery>
    {
        public GetIncrementValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Key must be provided.")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Key must only contain letters, digits, or underscores.");
        }
    }

    public async Task<IncrementKey> Handle(GetIncrementQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Entering {nameof(Handle)} method of class {_className}");

        GetIncrementValidator validator = new();
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (validationResult.IsValid is false)
        {   // TODO -- improve/consolidate error reporting?
            string errors = string.Join("; ", validationResult.Errors);
            _logger.LogWarning("Validation failed for GetIncrementQuery: {Errors}", errors);
            throw new BadRequestException(errors);
        }

        IncrementKey? result = await _repository.GetIncrement(request.Key).ConfigureAwait(false);

        return result ?? throw new NotFoundException($"Increment key '{request.Key}' not found.");
    }
}
