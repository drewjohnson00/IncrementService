using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Repository;

namespace IncrementService.Handlers;

public class GetAllIncrementsQuery : IRequest<List<IncrementKey>> { }

public class GetAllIncrementsHandler(IIncrementRepository repository, ILogger<GetAllIncrementsHandler> logger)
    : BaseHandler<GetAllIncrementsHandler, GetAllIncrementsQuery>(logger)
        ,IRequestHandler<GetAllIncrementsQuery, List<IncrementKey>>
{
    public async Task<List<IncrementKey>> Handle(GetAllIncrementsQuery request, CancellationToken cancellationToken)
    {
        TraceLogging();

        return await repository.GetAllIncrements().ConfigureAwait(false);
    }
}
