using Blazor9.Auto.Client.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blazor9.Auto.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Person> People => Set<Person>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Location> Locations => Set<Location>();
    }
}
