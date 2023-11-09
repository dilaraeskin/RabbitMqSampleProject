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
    public class SourceDataContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public SourceDataContext(DbContextOptions<SourceDataContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
            var asd = _configuration.GetConnectionString("SourceConnectionString");
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