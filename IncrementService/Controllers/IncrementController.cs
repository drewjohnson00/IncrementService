using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IncrementService.Handlers;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace IncrementService.Controllers;

//[Authorize]
[ApiController]
[Route("Increment")]
public class IncrementController : ControllerBase
{
    private readonly ILogger<IncrementController> _logger;
    private readonly IMediator _mediator;
    private readonly string _tracelog = "In IncrementController, Entering method: {Method}";

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

        List<IncrementKey> dto = await _mediator.Send(new GetAllIncrementsQuery()).ConfigureAwait(false);
        return Ok(dto);
    }

    /// <summary>
    /// Get a specific Increment by key
    /// </summary>
    /// <param name="query"></param>
    [HttpGet("{query}")]
    public async Task<ActionResult<IncrementKey>> Get([FromRoute] GetIncrementQuery query)
    {
        _logger.LogInformation(_tracelog, nameof(Get));

        //if (VerifyIncrementKey(query.Key) is false)
        //{
        //    _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Get), $"Increment Key '{query.Key}' is not valid..must only include letters, digits, or underscores.");
        //    return BadRequest($"Increment Key '{query.Key}' is not valid...must only include letters, digits, or underscores.");
        //}

        IncrementKey dto = await _mediator.Send(query);
        return Ok(dto);
    }

    /// <summary>
    /// Upsert an Increment
    /// </summary>
    /// <param name="key"></param>
    /// <param name="previousValue"></param>
    /// <returns></returns>
    [HttpPut("{key}")]
    public async Task<ActionResult<IncrementKey>> Put(string key, [FromQuery] long? previousValue = 0)
    {
        _logger.LogInformation(_tracelog, nameof(Put));

        //if (VerifyIncrementKey(key) is false)
        //{
        //    string errorMessage = $"Increment Key '{key}' is not valid. Key must only include letters, digits, or underscores.";
        //    _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), errorMessage);
        //    return BadRequest(errorMessage);
        //}

        //if (previousValue < 0)
        //{
        //    _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), "PreviousValue must be greater than or equal to 0.");
        //    return BadRequest("PreviousValue must be greater than or equal to 0.");
        //}

        PutIncrementCommand command = new ()
        {
            Key = key,
            PreviousValue = previousValue
        };

        IncrementKey dto = await _mediator.Send(command);

        _logger.LogInformation("Exiting {Method}...success!", nameof(Put));
        return Ok(dto);
    }

    //[Route("{key}")]
    //[HttpPost]
    //public ActionResult Post(string key)
    //{
    //    _logger.LogInformation("Entering {Method} with key: {Key}.", nameof(Post), key);

    //    if (!VerifyIncrementKey(key))
    //    {
    //        _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Post), "Increment Key is not valid.");
    //        return BadRequest("Increment Key is not valid.");
    //    }

    //    //ModelResponse result = null;

    //    //try
    //    //{
    //    //    result = _model.Increment(key);
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Post));
    //    //    return StatusCode(StatusCodes.Status500InternalServerError);
    //    //}

    //    //if (result.IsSuccess)
    //    //{
    //    //    _logger.LogInformation("Exiting {Method}...success!", nameof(Post));
    //    //    return Ok(result.Results[0].PreviousValue);
    //    //}

    //    //_logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Post), "Key not found.");
    //    //return NotFound(result.ErrorMessage);   // TODO -- Change message if user isn't admin?
    //}

    //// Add a new Increment
    //[Route("{key}/{initialCount=1}")]
    //[HttpPut]
    //public ActionResult Put(string key, long initialCount = 1)
    //{
    //    _logger.LogInformation("Entering {Method} with key: {Key} and initialCount: {Count}.", nameof(Put), key, initialCount);

    //    if (!VerifyIncrementKey(key))
    //    {
    //        _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), "Increment Key is not valid.");
    //        return BadRequest("Increment Key is not valid.");
    //    }

    //    if (initialCount < 1)
    //    {
    //        _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), "Invalid Initial Count.");
    //        return BadRequest("Invalid Initial Count.");
    //    }

    //    //ModelResponse result = null;

    //    //try
    //    //{
    //    //    result = _model.AddIncrement(key, initialCount);
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Put));
    //    //    return StatusCode(StatusCodes.Status500InternalServerError);
    //    //}

    //    //if (result.IsSuccess)
    //    //{
    //    //    _logger.LogInformation("Exiting {Method}...success!", nameof(Put));
    //    //    return Created(new Uri($"{this.Request.Scheme}://{this.Request.Host}/Increment/{key}"), null);
    //    //}

    //    //_logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), result.ErrorMessage);
    //    //return Ok(result.ErrorMessage);
    //}


    //[Route("{key}")]
    //[HttpDelete]
    //public ActionResult Delete(string key)
    //{
    //    _logger.LogInformation("Entering {Method} with key: {Key}.", nameof(Delete), key);
    //    if (!VerifyIncrementKey(key))
    //    {
    //        _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Delete), "Increment Key is not valid.");
    //        return BadRequest("Increment Key is not valid.");
    //    }

    //    //ModelResponse result = null;

    //    //try
    //    //{
    //    //    result = _model.RemoveIncrement(key);
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Delete));
    //    //    return StatusCode(StatusCodes.Status500InternalServerError);
    //    //}

    //    //if (!result.IsSuccess)
    //    //{
    //    //    _logger.LogInformation("Exiting {Method}...success!", nameof(Delete));
    //    //    return Ok();
    //    //}

    //    //_logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Delete), result.ErrorMessage);
    //    //return Ok(result.ErrorMessage);
    //}

    /// <summary>
    /// Enforce that the increment key contains only word characters (letters, digits, or underscores).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static bool VerifyIncrementKey(string key)
    {
        Match match = Regex.Match(key, @"\W");  // \W matches any non-word character...so if there is a match, it's not valid.
        return !match.Success;
    }

}
