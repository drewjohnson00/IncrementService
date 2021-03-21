using System;
using System.Text.RegularExpressions;
using IncrementService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;


namespace IncrementService.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class IncrementController : ControllerBase
    {
        private readonly ILogger<IncrementController> _logger;
        private readonly IIncrementModel _model;

        public IncrementController(ILogger<IncrementController> logger, IIncrementModel model)
        {
            _logger = logger;
            _model = model;
        }

        [HttpGet]
        public ActionResult<IncrementRow> Get()
        {
            _logger.LogInformation("Entering {Method}", nameof(Get));
            //if (this.HttpContext.Request.Cookies.Count == 0)
            //{
            //    return NotFound("Call login first!");
            //}

            ModelResponse result = null;

            try
            {
                result = _model.GetAllIncrements();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Get));
                return StatusCode(StatusCodes.Status500InternalServerError); // TODO -- Include exception if in development mode
            }

            if (result.IsSuccess)
            {
                _logger.LogInformation("Exiting {Method}...success!", nameof(Get));
                return Ok(result.Results);
            }

            _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Get), result.ErrorMessage);
            return NotFound(result.ErrorMessage);
        }

        [Route("{key}")]
        [HttpGet]
        public ActionResult<IncrementRow> Get(string key)
        {
            _logger.LogInformation("Entering {Method} with key: {Key}.", nameof(Get), key);

            if (!VerifyIncrementKey(key))
            {
                _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Get), "Increment Key is not valid.");
                return BadRequest("Increment Key is not valid.");
            }

            ModelResponse result = null;

            try
            {
                result = _model.GetIncrement(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Get));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (result.IsSuccess)
            {
                _logger.LogInformation("Exiting {Method}...success!", nameof(Get));
                return Ok(result.Results);
            }

            _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Get), "Key not found.");
            return NotFound(result.ErrorMessage);
        }

        [Route("{key}")]
        [HttpPost]
        public ActionResult Post(string key)
        {
            _logger.LogInformation("Entering {Method} with key: {Key}.", nameof(Post), key);

            if (!VerifyIncrementKey(key))
            {
                _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Post), "Increment Key is not valid.");
                return BadRequest("Increment Key is not valid.");
            }

            ModelResponse result = null;

            try
            {
                result = _model.Increment(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Post));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (result.IsSuccess)
            {
                _logger.LogInformation("Exiting {Method}...success!", nameof(Post));
                return Ok(result.Results[0].PreviousValue);
            }

            _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Post), "Key not found.");
            return NotFound(result.ErrorMessage);   // TODO -- Change message if user isn't admin?
        }

        // Add a new Increment
        [Route("{key}/{initialCount=1}")]
        [HttpPut]
        public ActionResult Put(string key, long initialCount = 1)
        {
            _logger.LogInformation("Entering {Method} with key: {Key} and initialCount: {Count}.", nameof(Put), key, initialCount);

            if (!VerifyIncrementKey(key))
            {
                _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), "Increment Key is not valid.");
                return BadRequest("Increment Key is not valid.");
            }

            if (initialCount < 1)
            {
                _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), "Invalid Initial Count.");
                return BadRequest("Invalid Initial Count.");
            }

            ModelResponse result = null;

            try
            {
                result = _model.AddIncrement(key, initialCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Put));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (result.IsSuccess)
            {
                _logger.LogInformation("Exiting {Method}...success!", nameof(Put));
                return Created(new Uri($"{this.Request.Scheme}://{this.Request.Host}/Increment/{key}"), null);
            }

            _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Put), result.ErrorMessage);
            return Ok(result.ErrorMessage);
        }


        [Route("{key}")]
        [HttpDelete]
        public ActionResult Delete(string key)
        {
            _logger.LogInformation("Entering {Method} with key: {Key}.", nameof(Delete), key);
            if (!VerifyIncrementKey(key))
            {
                _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Delete), "Increment Key is not valid.");
                return BadRequest("Increment Key is not valid.");
            }

            ModelResponse result = null;

            try
            {
                result = _model.RemoveIncrement(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown in method {Method}.", ex, nameof(Delete));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (!result.IsSuccess)
            {
                _logger.LogInformation("Exiting {Method}...success!", nameof(Delete));
                return Ok();
            }

            _logger.LogWarning("Exiting {Method} with error: {Error}", nameof(Delete), result.ErrorMessage);
            return Ok(result.ErrorMessage);
        }

        private static bool VerifyIncrementKey(string key)
        {
            Match match = Regex.Match(key, @"\W");  // \W matches any non-word character...so if there is a match, it's not valid.
            return !match.Success;
        }

    }
}
