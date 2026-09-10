using Microsoft.EntityFrameworkCore;
namespace Gs1.DigitalLink.Resolver;
public static class ResolverDatabase
{
    public const string ConnectionStringEnvironmentVariable = "GS1_RESOLVER_CONNECTION_STRING";
    public static ResolverDbContext CreateFromEnvironment()
    {
        string connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? throw new InvalidOperationException(
                $"Set the {ConnectionStringEnvironmentVariable} environment variable before connecting to PostgreSQL.");
        var options = new DbContextOptionsBuilder<ResolverDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new ResolverDbContext(options);
    }
    public static async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using ResolverDbContext context = CreateFromEnvironment();
        await context.Database.MigrateAsync(cancellationToken);
    }
}