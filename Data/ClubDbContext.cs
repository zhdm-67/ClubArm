// Data/ClubDbContext.cs
using Microsoft.EntityFrameworkCore;
using ClubArm.Models;

namespace ClubArm.Data
{
    public class ClubDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Data Source=WIN-9U2BGSFIHLG\SQLEXPRESS;Initial Catalog=ClubArmDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Tariff>().HasData(
                new Tariff { Id = 1, Name = "Стандарт", Type = "Minute", PricePerMinute = 0.05m },
                new Tariff { Id = 2, Name = "Ночной", Type = "Night", PricePerMinute = 0.03m }
            );
            modelBuilder.Entity<Computer>().HasData(
                new Computer { Id = 1, Name = "PC-1", Status = "Free", Configuration = "i5/16GB/RTX3060" },
                new Computer { Id = 2, Name = "PC-2", Status = "Free", Configuration = "i7/32GB/RTX3070" },
                new Computer { Id = 3, Name = "PC-3", Status = "Free", Configuration = "i5/16GB/GTX1660" }
            );
        }
    }
}