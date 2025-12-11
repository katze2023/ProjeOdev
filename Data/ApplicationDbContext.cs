using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Models;

namespace FitnessCenterManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Salon> Salons { get; set; }
        public DbSet<Service> Services { get; set; }   
        public DbSet<Trainer> Trainers { get; set; }
        //public DbSet<Appointment> Appointments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Örnek: Service tablosu için ek konfig.
            builder.Entity<Service>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });
        }
    }
}
