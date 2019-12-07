using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IncrementService.Data;

namespace IncrementService.Models
{
    public class IncrementModel : IIncrementData
    {

        public IncrementModel()
        {
        }


        public DataResultDto AddIncrement(string IncrementKey, long initialValue)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DateTime.Now, NextVaue = initialValue }
    });
        }

        public DataResultDto GetAllIncrements()
        {
            return new DataResultDto(true, 0, "", 
                new List<IncrementDto> { new IncrementDto { Key = "One", LastUsed = DateTime.Now, NextVaue = 5 },
                        new IncrementDto { Key = "Two", LastUsed = DateTime.Now, NextVaue = 42 }
                });

            //using (var db = new IncrementContext())
            //{
            //    db.Find<IncrementDto>()
            //}







            //using (var conn = new SqlConnection(connString))
            //{
            //    using (var cmd = new SqlCommand("SELECT * FROM Keys", conn))
            //    {
            //        conn.Open();
            //        SqlDataReader reader = cmd.ExecuteReader();
            //        while (reader.Read())
            //        {
            //            IncrementDto inc = new IncrementDto
            //            {
            //                Key = reader["IncrementKey"].ToString().Trim(' '),
            //                NextVaue = (long)reader["NextValue"],
            //                LastUsed = (DateTimeOffset)reader["LastUsed"]

            //            };
            //            resultList.Add(inc);
            //        }
            //        conn.Close();
            //    }
            //}
        }

        public DataResultDto GetIncrement(string IncrementKey)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DateTime.Now, NextVaue = 42 }
                });

        }

        public DataResultDto Increment(string IncrementKey)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DateTime.Now, NextVaue = 42 }
                });
        }

        public DataResultDto RemoveIncrement(string IncrementKey)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DateTime.Now, NextVaue = 0 }
    });
        }
    }
}
