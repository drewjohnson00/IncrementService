using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IncrementService
{
    public partial class IncrementServiceContext : DbContext
    {
        public IncrementServiceContext()
        {
        }

        public IncrementServiceContext(DbContextOptions<IncrementServiceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Keys> Keys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=IncrementService;Integrated Security=True;MultipleActiveResultSets=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Keys>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_Keys_IncrementKeyUnique")
                    .IsUnique();

                entity.Property(e => e.IncrementKey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
