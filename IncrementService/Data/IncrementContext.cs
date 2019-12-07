using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using IncrementService.Models;
using Microsoft.EntityFrameworkCore;

namespace IncrementService.Data
{
    public class IncrementContext : DbContext
    {
        public DbSet<IncrementDto> Increments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options
            .UseSqlServer(GetConnectionString(), providerOptions => providerOptions.CommandTimeout(10))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);


        private static string GetConnectionString()
        {        // TODO -- Get this from the web.config
            return "Data Source=.;Initial Catalog=IncrementService;Integrated Security=True;MultipleActiveResultSets=True";
        }
    }
}
