using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class GetAllIncrementsQuery : IRequest<List<IncrementKey>>
{

}

public class GetAllIncrementsHandler : IRequestHandler<GetAllIncrementsQuery, List<IncrementKey>>
{
    private readonly IIncrementRepository _repository;
    private readonly ILogger<GetAllIncrementsHandler> _logger;
    private readonly string _className = nameof(GetAllIncrementsHandler);

    public GetAllIncrementsHandler(IIncrementRepository repository, ILogger<GetAllIncrementsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<IncrementKey>> Handle(GetAllIncrementsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Entering {nameof(Handle)} method of class {_className}");

        return await _repository.GetAllIncrements().ConfigureAwait(false);

        //if (result.IsSuccess)
        //{
        //    return Task.FromResult(result.Results);
        //}
        //else
        //{
        //    // In a real application, you might want to throw an exception or handle the error differently
        //    return Task.FromResult(new List<IncrementKey>());
        //}
    }
}
