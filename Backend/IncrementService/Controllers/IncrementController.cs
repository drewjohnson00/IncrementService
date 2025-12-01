using System.Collections.Generic;
using System.Threading.Tasks;
using IncrementService.Handlers;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IncrementService.Controllers;

[ApiController]
[Route("Increments")]
public class IncrementsController(ILogger<IncrementsController> logger, IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all Increments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IncrementKey>> Get()
    {
        // The empty logInfo method below output the following:
        // [INF] (IncrementService.Controllers.IncrementsController) {"ActionId":"<guid>","ActionName":"IncrementService.Controllers.IncrementsController.Get (IncrementService)","RequestId":"<string>:<index>","RequestPath":"/increments","ConnectionId":"<string>"}
        // This is sufficient for tracing the request through the system.
        logger.LogInformation(""); 

        // MediatR routes this to the GetAllIncrementsHandler because of the GetAllIncrementsQuery type that implements IRequest<List<IncrementKey>>
        List<IncrementKey> dto = await mediator.Send(new GetAllIncrementsQuery()).ConfigureAwait(false);
        return Ok(dto);
    }

    /// <summary>
    /// Get a specific Increment by key
    /// </summary>
    /// <param name="query"></param>
    [HttpGet("{Key}")]
    public async Task<ActionResult<IncrementKey>> Get([FromRoute] GetIncrementQuery query)
    {
        logger.LogInformation("");

        IncrementKey dto = await mediator.Send(query);
        return Ok(dto);
    }

    /// <summary>
    /// Upsert an Increment
    /// </summary>
    /// <param name="command"></param>
    [HttpPut]
    public async Task<ActionResult<IncrementKey>> Put([FromBody] PutIncrementCommand command)
    {
        logger.LogInformation("");

        IncrementKey dto = await mediator.Send(command);

        logger.LogInformation("Exiting {Method}...success!", nameof(Put));
        return Ok(dto);
    }

    [HttpPost("{key}")]
    public async Task<ActionResult<IncrementKey>> Post([FromRoute] string key)
    {
        logger.LogInformation("");

        IncrementKey dto = await mediator.Send(new PostIncrementCommand() { Key = key });
        return Ok(dto);
    }

    [HttpDelete("{Key}")]
    public async Task<ActionResult> Delete([FromRoute] DeleteIncrementCommand command)
    {
        logger.LogInformation("");

        await mediator.Send(command);

        logger.LogInformation("Exiting {Method}...success!", nameof(Delete));
        return Ok();
    }
}
