using System.Threading;
using System.Threading.Tasks;
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
            .NotEmpty().WithMessage("Key must be provided.");
    }
}

public class DeleteIncrementHandler(IIncrementRepository repository, ILogger<DeleteIncrementHandler> logger)
    : BaseHandler<DeleteIncrementHandler, DeleteIncrementCommand>(logger, new DeleteIncrementValidator())
        , IRequestHandler<DeleteIncrementCommand>
{
    public async Task Handle(DeleteIncrementCommand command, CancellationToken cancellationToken)
    {
        await ExecValidationsAndThrowOnErrorAsync(command, cancellationToken).ConfigureAwait(false);

        await repository.DeleteIncrement(command.Key).ConfigureAwait(false);
    }
}
