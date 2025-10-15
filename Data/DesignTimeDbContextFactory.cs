using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SkyQuizApp.Data;

namespace SkyQuizApp
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SkyQuizDb;Trusted_Connection=True");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
