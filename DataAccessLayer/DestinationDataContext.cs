using DataContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    public class DestinationDataContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DestinationDataContext(DbContextOptions<DestinationDataContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
            Database.EnsureCreated();
        }
        public DbSet<Target> Targets { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Target>().ToTable("tbl_Target"); 
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DestinationConnectionString"));
            }
        }
    }
}