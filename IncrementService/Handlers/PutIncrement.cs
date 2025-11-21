using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Common.Exceptions;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class PutIncrementCommand : IncrementCommand, IRequest<IncrementKey> { };

internal class PutIncrementValidator : AbstractValidator<PutIncrementCommand>
{
    public PutIncrementValidator()
    {
        RuleFor(x => x.Key)
            .Length(3, 50).WithMessage("Key must be between 3 and 50 characters.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Key must only contain letters, digits, or underscores.");
        RuleFor(x => x.PreviousValue)
            .Must(previousValue => !previousValue.HasValue || previousValue.Value >= 0)
            .WithMessage("PreviousValue must be greater than or equal to 0.");
    }
}

public class PutIncrementHandler(IIncrementRepository repository, ILogger<PutIncrementHandler> logger)
    : BaseHandler<PutIncrementHandler, PutIncrementCommand>(logger, new PutIncrementValidator())
        , IRequestHandler<PutIncrementCommand, IncrementKey>
{
    public async Task<IncrementKey> Handle(PutIncrementCommand command, CancellationToken cancellationToken)
    {
        await ExecValidationsAndThrowOnErrorAsync(command, cancellationToken).ConfigureAwait(false);

        IncrementKey result = await repository.UpsertIncrement(command).ConfigureAwait(false);
        return result;
    }
}
