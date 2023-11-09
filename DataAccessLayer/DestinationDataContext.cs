using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataContract;
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