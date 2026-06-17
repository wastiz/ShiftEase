using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=shift_ease;Username=postgres;Password=admin");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}