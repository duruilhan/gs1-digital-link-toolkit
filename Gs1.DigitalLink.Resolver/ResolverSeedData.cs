using Microsoft.EntityFrameworkCore;
namespace Gs1.DigitalLink.Resolver;
internal static class ResolverSeedData
{
    private static readonly Guid GtinDefinitionId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid LotDefinitionId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid GlnDefinitionId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    internal static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LinkDefinition>().HasData(
            new { Id = GtinDefinitionId, CanonicalPath = "/01/08690504080008" },
            new { Id = LotDefinitionId, CanonicalPath = "/01/08690504080008/10/LOT123" },
            new { Id = GlnDefinitionId, CanonicalPath = "/414/8690123456789" });
        modelBuilder.Entity<LinkIdentifier>().HasData(
            Identifier("20000000-0000-0000-0000-000000000001", GtinDefinitionId, 0, "01", "08690504080008"),
            Identifier("20000000-0000-0000-0000-000000000002", LotDefinitionId, 0, "01", "08690504080008"),
            Identifier("20000000-0000-0000-0000-000000000003", LotDefinitionId, 1, "10", "LOT123"),
            Identifier("20000000-0000-0000-0000-000000000004", GlnDefinitionId, 0, "414", "8690123456789"));
        modelBuilder.Entity<LinkTarget>().HasData(
            Target("30000000-0000-0000-0000-000000000001", GtinDefinitionId, "gs1:pip", "https://example.com/tr/products/08690504080008", "tr", "text/html", false),
            Target("30000000-0000-0000-0000-000000000002", GtinDefinitionId, "gs1:pip", "https://example.com/en/products/08690504080008", "en", "text/html", false),
            Target("30000000-0000-0000-0000-000000000003", GtinDefinitionId, "gs1:defaultLink", "https://example.com/products/08690504080008", null, "text/html", true),
            Target("30000000-0000-0000-0000-000000000004", LotDefinitionId, "gs1:pip", "https://example.com/tr/products/08690504080008/lots/LOT123", "tr", "text/html", false),
            Target("30000000-0000-0000-0000-000000000005", GlnDefinitionId, "gs1:defaultLink", "https://example.com/locations/8690123456789", null, "text/html", true));
    }
    private static object Identifier(string id, Guid definitionId, int position, string ai, string value) =>
        new { Id = Guid.Parse(id), LinkDefinitionId = definitionId, Position = position, ApplicationIdentifier = ai, Value = value };
    private static object Target(string id, Guid definitionId, string type, string url,
        string? language, string mediaType, bool isDefault) =>
        new { Id = Guid.Parse(id), LinkDefinitionId = definitionId, LinkType = type, Url = url, Language = language, MediaType = mediaType, IsDefault = isDefault };
}