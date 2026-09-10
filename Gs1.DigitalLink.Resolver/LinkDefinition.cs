namespace Gs1.DigitalLink.Resolver;
public sealed class LinkDefinition
{
    public Guid Id { get; set; }
    public required string CanonicalPath { get; set; }
    public List<LinkIdentifier> Identifiers { get; set; } = [];
    public List<LinkTarget> Targets { get; set; } = [];
}
public sealed class LinkIdentifier
{
    public Guid Id { get; set; }
    public Guid LinkDefinitionId { get; set; }
    public LinkDefinition LinkDefinition { get; set; } = null!;
    public int Position { get; set; }
    public required string ApplicationIdentifier { get; set; }
    public required string Value { get; set; }
}
public sealed class LinkTarget
{
    public Guid Id { get; set; }
    public Guid LinkDefinitionId { get; set; }
    public LinkDefinition LinkDefinition { get; set; } = null!;
    public required string LinkType { get; set; }
    public required string Url { get; set; }
    public string? Language { get; set; }
    public string? MediaType { get; set; }
    public bool IsDefault { get; set; }
}