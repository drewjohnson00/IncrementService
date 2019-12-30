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

        public IncrementContext(DbContextOptions<IncrementContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options == null)
            {
                throw new ArgumentException("DbContextOptionsBuilder parameter cannot be NULL.");
            }
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        //.UseSqlServer(GetConnectionString(), providerOptions => providerOptions.CommandTimeout(10))


        private static string GetConnectionString()
        {        // TODO -- Get this from the web.config
            return "Data Source=.;Initial Catalog=IncrementService;Integrated Security=True;MultipleActiveResultSets=True";
        }
    }
}
