using System.Text.Json;
namespace Gs1.DigitalLink
{
public static class ApplicationIdentifierCatalog
{
    private static readonly Lazy<IReadOnlyDictionary<string, ApplicationIdentifierDefinition>> Definitions =
        new(LoadDefinitions);
    public static bool TryGet(string? code, out ApplicationIdentifierDefinition? definition)
    {
        if (code is null)
        {
            definition = null;
            return false;
        }
        return Definitions.Value.TryGetValue(code, out definition);
    }
    internal static bool TryMatchPrefix(
        string input,
        int position,
        out ApplicationIdentifierDefinition? definition)
    {
        definition = Definitions.Value.Values
            .Where(candidate => position + candidate.Code.Length <= input.Length)
            .OrderByDescending(candidate => candidate.Code.Length)
            .FirstOrDefault(candidate => input.AsSpan(position).StartsWith(candidate.Code, StringComparison.Ordinal));

        return definition is not null;
    }
    private static IReadOnlyDictionary<string, ApplicationIdentifierDefinition> LoadDefinitions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "application-identifiers.json");
        string json = File.ReadAllText(path);
        var definitions = JsonSerializer.Deserialize<List<ApplicationIdentifierDefinition>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Application Identifier definitions could not be loaded.");
        return definitions.ToDictionary(definition => definition.Code, StringComparer.Ordinal);
    }
}
}
