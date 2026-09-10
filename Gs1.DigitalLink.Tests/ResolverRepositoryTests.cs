using Gs1.DigitalLink.Resolver;
using Microsoft.EntityFrameworkCore;
namespace Gs1.DigitalLink.Tests;
public sealed class ResolverRepositoryTests
{
    [Fact]
    public async Task SeedData_CanRetrieveDefinitionAndAllTargetsByCanonicalPath()
    {
        await using ResolverDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var repository = new ResolverRepository(context);
        LinkDefinition? definition = await repository.GetByCanonicalPathAsync("/01/08690504080008");
        Assert.NotNull(definition);
        Assert.Equal("01", Assert.Single(definition.Identifiers).ApplicationIdentifier);
        Assert.Equal(3, definition.Targets.Count);
        Assert.Contains(definition.Targets, target => target.LinkType == "gs1:pip" && target.Language == "tr");
        Assert.Contains(definition.Targets, target => target.LinkType == "gs1:pip" && target.Language == "en");
        Assert.Contains(definition.Targets, target => target.LinkType == "gs1:defaultLink" && target.IsDefault);
    }
    [Fact]
    public async Task SeedData_KeepsQualifiedAndUnqualifiedGtinAsSeparateDefinitions()
    {
        await using ResolverDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var repository = new ResolverRepository(context);
        LinkDefinition? unqualified = await repository.GetByCanonicalPathAsync("/01/08690504080008");
        LinkDefinition? qualified = await repository.GetByCanonicalPathAsync("/01/08690504080008/10/LOT123");
        Assert.NotNull(unqualified);
        Assert.NotNull(qualified);
        Assert.NotEqual(unqualified.Id, qualified.Id);
        Assert.Equal(["01", "10"], qualified.Identifiers.Select(item => item.ApplicationIdentifier));
        Assert.Single(qualified.Targets);
    }
    [Fact]
    public async Task SeedData_ContainsGlnDefaultLink()
    {
        await using ResolverDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        var repository = new ResolverRepository(context);
        LinkDefinition? definition = await repository.GetByCanonicalPathAsync("/414/8690123456789");
        Assert.NotNull(definition);
        LinkTarget target = Assert.Single(definition.Targets);
        Assert.Equal("gs1:defaultLink", target.LinkType);
        Assert.True(target.IsDefault);
    }
    [Fact]
    public void PostgreSqlModel_ContainsInitialMigrationWithoutConnecting()
    {
        var options = new DbContextOptionsBuilder<ResolverDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;
        using var context = new ResolverDbContext(options);
        Assert.Contains("202609100001_InitialResolverSchema", context.Database.GetMigrations());
    }
    private static ResolverDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ResolverDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ResolverDbContext(options);
    }
}