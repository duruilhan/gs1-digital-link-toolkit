namespace Gs1.DigitalLink.Resolver.Api;

public sealed record ElementRequest(string ApplicationIdentifier, string Value);

public sealed record TargetRequest(
    string LinkType,
    string Url,
    string? Language,
    string? MediaType,
    bool IsDefault);

public sealed record CreateDefinitionRequest(
    IReadOnlyList<ElementRequest> Elements,
    IReadOnlyList<TargetRequest> Targets);
