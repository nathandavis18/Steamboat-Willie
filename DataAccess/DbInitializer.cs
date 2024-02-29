using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class DbInitializer : IDbInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Initialize()
        {
            _context.Database.EnsureCreated();

            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception)
            {

            }

            _context.SaveChanges();
        }
    }
}
