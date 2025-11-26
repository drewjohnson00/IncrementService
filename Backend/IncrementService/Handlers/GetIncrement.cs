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

internal class GetIncrementValidator : AbstractValidator<GetIncrementQuery>
{
    public GetIncrementValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key must be provided.");
    }
}

public class GetIncrementHandler(IIncrementRepository repository, ILogger<GetIncrementHandler> logger)
    : BaseHandler<GetIncrementHandler, GetIncrementQuery>(logger, new GetIncrementValidator())
        ,IRequestHandler<GetIncrementQuery, IncrementKey>
{
    public async Task<IncrementKey> Handle(GetIncrementQuery request, CancellationToken cancellationToken)
    {
        await ExecValidationsAndThrowOnErrorAsync(request, cancellationToken).ConfigureAwait(false);

        IncrementKey? result = await repository.GetIncrement(request.Key).ConfigureAwait(false);

        return result ?? throw new NotFoundException($"Increment key '{request.Key}' not found.");
    }
}
