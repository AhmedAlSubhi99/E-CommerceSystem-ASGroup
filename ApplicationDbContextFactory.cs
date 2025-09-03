using E_CommerceSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Load configuration (works for both design time and run time)
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var cs = config.GetConnectionString("DefaultConnection");
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(cs)
            .Options;
        return new ApplicationDbContext(opts);
    }
}