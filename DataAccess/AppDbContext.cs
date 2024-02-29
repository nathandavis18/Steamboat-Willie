using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
