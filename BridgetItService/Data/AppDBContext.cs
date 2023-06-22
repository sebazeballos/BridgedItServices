using BridgetItService.Models;
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

        public DbSet<DbProduct> Products { get; set; }
    }
}
