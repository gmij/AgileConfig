using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Data.EFCore;

public class AgileConfigDbContextFactory : IDesignTimeDbContextFactory<AgileConfigDbContext>
{
    public AgileConfigDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AgileConfigDbContext>();

        // Use SQLite for design-time migrations (can be overridden at runtime)
        optionsBuilder.UseSqlite("Data Source=agile_config.db");

        return new AgileConfigDbContext(optionsBuilder.Options);
    }
}
