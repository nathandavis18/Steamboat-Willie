using Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> ApplicationUsers { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<AppointmentCategory> AppointmentsCategories { get; set;}

        public DbSet<Client> Clients { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public DbSet<ProviderAvailability> ProviderAvailability { get; set; }



    }
}
