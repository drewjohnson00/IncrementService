using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using IncrementService.Models;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace IncrementService.Controllers
{
    public class IncrementController : ApiController
    {
        // [Route("increment")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            var connString = GetConnectionString();
            List<string> resultList = new List<string>();

            using (var conn = new SqlConnection(connString))
            {
                var cmd = new SqlCommand("SELECT * FROM Keys", conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    resultList.Add(reader["IncrementKey"].ToString().Trim(' '));
                }
                conn.Close();
            }

            //return resultList;
            var result =  new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new JsonContent($"{JsonConvert.SerializeObject(resultList)}")
            };

            return ResponseMessage(result);
        }

        // [Route("increment/{key}")]
        [Route("{key}")]
        [HttpGet]
        public IHttpActionResult Get(string key)
        {
            var connString = GetConnectionString();
            long result = 0;

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand("IncrementKey", conn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@Key", SqlDbType.NVarChar, 50).Value = key;

                SqlParameter nv = new SqlParameter("@NewValue", SqlDbType.BigInt);
                nv.Direction = ParameterDirection.Output;
                nv.Value = result;
                cmd.Parameters.Add(nv);
                conn.Open();
                cmd.ExecuteNonQuery();
                result = (long)nv.Value;
                conn.Close();
            }

            if(result == -1)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        // [Route("increment")]
        [HttpPut]
        // PUT instance -- create a new increment
        public void Put([FromBody]Increment model)
        {
            var connString = GetConnectionString();
            //var connString = ConfigurationManager.ConnectionStrings["IncrementConnString"].ConnectionString;
            //var connString = WebConfigurationManager.ConnectionStrings[0].ConnectionString;

            string query = "INSERT INTO Keys (IncrementKey, NextValue, IsDeletable) VALUES (@IncrementKey, @NextValue, @IsDeletable)";
            using (var conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@IncrementKey", SqlDbType.NVarChar, 50).Value = model.Key;
                cmd.Parameters.Add("@NextValue", SqlDbType.BigInt).Value = model.InitialStart;
                cmd.Parameters.Add("@IsDeletable", SqlDbType.Char, 1).Value = model.IsDeletable ? "T" : "F";

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        [HttpPost]
        public void Post([FromBody]IncrementController model)
        {
            var connString = GetConnectionString();

            string query = null; // TODO -- call stored procedure here...

            return Ok();
        }

        // [Route("increment/{key}")]
        [Route("{key}")]
        [HttpDelete]
        // DELETE instance/key
        public void Delete(string id)
        {
            var connString = GetConnectionString();
            string query = "DELETE FROM dbo.Keys WHERE IncrementKey = @Key";
            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@Key", SqlDbType.NVarChar, 50).Value = id;

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

    }

}
