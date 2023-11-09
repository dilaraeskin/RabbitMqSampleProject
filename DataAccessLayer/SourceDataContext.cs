using DataContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    public class SourceDataContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public SourceDataContext(DbContextOptions<SourceDataContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
            Database.EnsureCreated();
        }
        public DbSet<Source> Sources { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Source>().ToTable("tbl_Source"); 
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("SourceConnectionString"));
            }
        }
    }
}