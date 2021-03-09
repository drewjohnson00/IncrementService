using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using IncrementService.Models;


namespace IncrementService.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Writing to Log.")]
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class IncrementController : ControllerBase
    {
        private readonly ILogger<IncrementController> _logger;
        private readonly IIncrementData _model;

        public IncrementController(ILogger<IncrementController> logger, IIncrementData model)
        {
            _logger = logger;
            _model = model;
        }

        [HttpGet]
        public ActionResult<IncrementDto> Get()
        {
            _logger.LogInformation($"Entering {nameof(Get)}.");
            if (this.HttpContext.Request.Cookies.Count == 0)
            {
                return NotFound("Call login first!");
            }

            DataResultDto result = _model.GetAllIncrements();

            if (result.IsSuccess)
            {
                _logger.LogInformation($"Exiting {nameof(Get)}. Success!");
                return Ok(result.Results);
            }

            _logger.LogWarning($"Exiting {nameof(Get)} with an error: {result.ErrorMessage}");
            return NotFound(result.ErrorMessage);   // TODO -- If I'm an admin, return a different message than otherwise.
        }

        //[Route("increment/{key}")]
        [Route("{key}")]
        [HttpGet]
        public ActionResult<IncrementDto> Get(string key)
        {
            //var connString = GetConnectionString();
            //long result = 0;

            //using (var conn = new SqlConnection(connString))
            //using (var cmd = new SqlCommand("IncrementKey", conn) { CommandType = CommandType.StoredProcedure })
            //{
            //    cmd.Parameters.Add("@Key", SqlDbType.NVarChar, 50).Value = key;

            //    SqlParameter nv = new SqlParameter("@NewValue", SqlDbType.BigInt);
            //    nv.Direction = ParameterDirection.Output;
            //    nv.Value = result;
            //    cmd.Parameters.Add(nv);
            //    conn.Open();
            //    cmd.ExecuteNonQuery();
            //    result = (long)nv.Value;
            //    conn.Close();
            //}

            bool isKeyValid = IncrementController.VerifyIncrementKey(key);
            if (!isKeyValid)
            {
                return BadRequest("Increment Key is not valid.");
            }

            DataResultDto result = _model.GetIncrement(key);

            if (result.IsSuccess)
            {
                return Ok(result.Results[0]);
            }

            return NotFound(result.ErrorMessage);
        }

        [Route("{key}")]
        [HttpPost]
        public ActionResult Post(string key)
        {
            bool isKeyValid = IncrementController.VerifyIncrementKey(key);
            if (!isKeyValid)
            {
                return BadRequest("Increment Key is not valid.");
            }

            DataResultDto result = _model.Increment(key);

            if(result.IsSuccess)
            {
                return Ok(result.Results[0].NextValue);
            }

            return NotFound(result.ErrorMessage);   // TODO -- Change message if user isn't admin?
        }

        // Add a new Increment
        [Route("{key}/{initialCount=1}")]
        [HttpPut]
        public ActionResult Put(string key, long initialCount = 1)
        {
            bool isKeyValid = IncrementController.VerifyIncrementKey(key);
            if (!isKeyValid)
            {
                return BadRequest("Increment Key is not valid.");
            }

            if (initialCount < 1)
            {
                return BadRequest("Invalid Initial Count.");
            }

            DataResultDto result = _model.AddIncrement(key, initialCount);

            if(!result.IsSuccess)
            {
                return Ok(result.ErrorMessage);
            }

            return Created(new Uri($"{this.Request.Scheme}://{this.Request.Host}/Increment/{key}"), null);
        }


        [Route("{key}")]
        [HttpDelete]
        public ActionResult Delete(string key)
        {
            bool isKeyValid = VerifyIncrementKey(key);
            if (!isKeyValid)
            {
                return BadRequest("Increment Key is not valid.");
            }

            DataResultDto result = _model.RemoveIncrement(key);
            if (!result.IsSuccess)
            {
                return Ok(result.ErrorMessage);
            }

            return Ok();
        }

        private static bool VerifyIncrementKey(string key)
        {
            Match match = Regex.Match(key, @"\W");  // \W matches any non-word character...so if there is a match, it's not valid.
            return !match.Success;
        }

    }
}
