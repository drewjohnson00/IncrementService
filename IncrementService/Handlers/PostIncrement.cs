using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class PostIncrementCommand : IRequest<IncrementKey>
{
    public required string Key { get; set; }
}

internal class PostIncrementValidator : AbstractValidator<PostIncrementCommand>
{
    public PostIncrementValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key must be provided.");
    }
}

public class PostIncrementHandler(IIncrementRepository repository, ILogger<PostIncrementHandler> logger)
    : BaseHandler<PostIncrementHandler, PostIncrementCommand>(logger, new PostIncrementValidator())
        ,IRequestHandler<PostIncrementCommand, IncrementKey>
{
    public async Task<IncrementKey> Handle(PostIncrementCommand command, CancellationToken cancellationToken)
    {
        await ExecValidationsAndThrowOnErrorAsync(command, cancellationToken).ConfigureAwait(false);

        IncrementKey result = await repository.PostIncrement(command.Key).ConfigureAwait(false);
        return result;
    }
}
