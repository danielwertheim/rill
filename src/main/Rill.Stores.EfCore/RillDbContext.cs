using Microsoft.EntityFrameworkCore;

namespace Rill.Stores.EfCore
{
    public class RillDbContext : DbContext
    {
        internal DbSet<RillEntity> Rills => Set<RillEntity>();
        internal DbSet<RillCommitEntity> Commits => Set<RillCommitEntity>();

        public RillDbContext(DbContextOptions<RillDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new RillEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RillCommitEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RillEventEntityConfiguration());
        }
    }
}
