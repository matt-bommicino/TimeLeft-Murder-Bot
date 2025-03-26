using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MurderBot.Data.Context;

public class MurderContextDesignTimeFactory : IDesignTimeDbContextFactory<MurderContext>
{
    public MurderContext CreateDbContext(string[] args)
    {
        // Build configuration from user secrets and environment variables
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<MurderContextDesignTimeFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        
        // Read the connection string
        var connectionString = configuration.GetSection("CommonMurderSettings")["MurderContextConnectionString"];

        // Configure the DbContext
        var optionsBuilder = new DbContextOptionsBuilder<MurderContext>()
            .UseSqlServer(connectionString);

        return new MurderContext(optionsBuilder.Options);
    }
}