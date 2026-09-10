using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Gs1.DigitalLink.Resolver;
public sealed class ResolverDbContextFactory : IDesignTimeDbContextFactory<ResolverDbContext>
{
    public ResolverDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("GS1_RESOLVER_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Set the GS1_RESOLVER_CONNECTION_STRING environment variable before using EF Core tools.");
        var options = new DbContextOptionsBuilder<ResolverDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new ResolverDbContext(options);
    }
}