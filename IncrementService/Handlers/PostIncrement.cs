using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using FluentValidation;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class PostIncrementCommand : IncrementCommand, IRequest<IncrementKey>;

internal class PostIncrementValidator : AbstractValidator<PostIncrementCommand>
{
    public PostIncrementValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key must be provided.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Key must only contain letters, digits, or underscores.");
    }
}

public class PostIncrementHandler(IIncrementRepository repository, ILogger<PostIncrementHandler> logger)
    : IRequestHandler<PostIncrementCommand, IncrementKey>
{
    private readonly string _className = nameof(PostIncrementHandler);


    public async Task<IncrementKey> Handle(PostIncrementCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering {Method} method of class {Class}", nameof(Handle), _className);

        PostIncrementValidator validator = new();
        var validationResult = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            string errors = string.Join("; ", validationResult.Errors);
            logger.LogWarning("Validation failed for {Method} in class {ClassName}: {Errors}",
                nameof(Handle), _className, errors);
            throw new BadRequestException($"Validation failed: {errors}");
        }

        IncrementKey result = await repository.PostIncrement(command).ConfigureAwait(false);
        return result;
    }
}
