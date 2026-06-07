using KeyManagment.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KeyManagment.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Building> Buildings { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<KeyHandover> KeyHandovers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // داده‌های اولیه — دو ساختمان نمونه
            builder.Entity<Building>().HasData(
                new Building { Id = 1, Name = "ساختمان الف", Description = "ساختمان اداری اصلی" },
                new Building { Id = 2, Name = "ساختمان ب", Description = "ساختمان فنی" }
            );
        }
    }
}