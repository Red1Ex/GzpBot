using Microsoft.EntityFrameworkCore;
using Gzpbot.Models;

namespace Gzpbot.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Competency> Competencies { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=helper;Username=postgres;Password=123");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Competency>().ToTable("Competencies");
            modelBuilder.Entity<Review>().ToTable("Reviews");
            modelBuilder.Entity<Vacancy>().ToTable("Vacancies");
        }
    }
}