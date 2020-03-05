using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BA.WebAPI.Model
{
    public class BikingDbContext : DbContext
    {
        public BikingDbContext(DbContextOptions<BikingDbContext> options) 
            : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<BikingEntry> BikingEntries { get; set; }

        public DbSet<WeeklyReport> WeeklyReports { get; set; }    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

    }
}
