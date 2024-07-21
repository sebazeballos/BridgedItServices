using BridgetItService.Models;
using BridgetItService.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace BridgetItService.Data
{
    public class BridgedItContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public BridgedItContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(Configuration.GetConnectionString("Db"));
        }

        public DbSet<DBProduct> Product { get; set; }
        public DbSet<DBTransaction> Transaction { get; set; }
        public DbSet<Health> Health { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DBProduct>().HasKey(p => p.Sku);
            modelBuilder.Entity<DBTransaction>().HasKey(p => p.InfinityTransactionId);
        }
    }
}
