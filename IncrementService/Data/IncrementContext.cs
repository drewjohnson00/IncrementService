using System;
using IncrementService.Models;
using Microsoft.EntityFrameworkCore;


namespace IncrementService.Data
{
    public class IncrementContext : DbContext
    {
        public DbSet<IncrementRow> Increments { get; set; }

        public IncrementContext(DbContextOptions<IncrementContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options == null)
            {
                throw new ArgumentException("DbContextOptionsBuilder parameter cannot be NULL.");
            }
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }
    }
}
