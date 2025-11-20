using System.Collections.Generic;
using System.Threading.Tasks;
using IncrementService.Handlers;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IncrementService.Controllers;

[ApiController]
[Route("Increment")]
public class IncrementController : ControllerBase
{
    private readonly ILogger<IncrementController> _logger;
    private readonly IMediator _mediator;
    private const string _tracelog = "In IncrementController, Entering method: {Method}";

    public IncrementController(ILogger<IncrementController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get all Increments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IncrementKey>> Get()
    {
        _logger.LogInformation(_tracelog, nameof(Get));

        // MediatR routes this to the GetAllIncrementsHandler because of the GetAllIncrementsQuery type that implements IRequest<List<IncrementKey>>
        List<IncrementKey> dto = await _mediator.Send(new GetAllIncrementsQuery()).ConfigureAwait(false);
        return Ok(dto);
    }

    /// <summary>
    /// Get a specific Increment by key
    /// </summary>
    /// <param name="query"></param>
    [HttpGet("{Key}")]
    public async Task<ActionResult<IncrementKey>> Get([FromRoute] GetIncrementQuery query)
    {
        _logger.LogInformation(_tracelog, nameof(Get));

        IncrementKey dto = await _mediator.Send(query);
        return Ok(dto);
    }

    /// <summary>
    /// Upsert an Increment
    /// </summary>
    /// <param name="command"></param>
    [HttpPut]
    public async Task<ActionResult<IncrementKey>> Put(PutIncrementCommand command)
    {
        _logger.LogInformation(_tracelog, nameof(Put));

        IncrementKey dto = await _mediator.Send(command);

        _logger.LogInformation("Exiting {Method}...success!", nameof(Put));
        return Ok(dto);
    }

    [HttpPost("{key}")]
    public async Task<ActionResult<IncrementKey>> Post(string key)
    {
        _logger.LogInformation("Entering {Method} with key: {Key}.", nameof(Post), key);

        IncrementKey dto = await _mediator.Send(new PostIncrementCommand() { Key = key });
        return Ok(dto);
    }

    [HttpDelete("{Key}")]
    public async Task<ActionResult> Delete([FromRoute] DeleteIncrementCommand command)
    {
        _logger.LogInformation(_tracelog, nameof(Delete));

        await _mediator.Send(command);

        _logger.LogInformation("Exiting {Method}...success!", nameof(Delete));
        return Ok();
    }
}
