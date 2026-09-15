namespace Gs1.DigitalLink.Resolver;
public static class TargetSelector
{
    public static LinkTarget? Select(
        IReadOnlyCollection<LinkTarget> targets,
        string? linkType,
        string? acceptLanguage,
        string? accept)
    {
        if (targets.Count == 0)
            return null;
        LinkTarget? defaultTarget = targets.SingleOrDefault(target => target.IsDefault);
        IEnumerable<LinkTarget> candidates = targets;
        if (!string.IsNullOrWhiteSpace(linkType))
        {
            LinkTarget[] matchingLinkType = candidates
                .Where(target => string.Equals(target.LinkType, linkType, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matchingLinkType.Length == 0)
                return defaultTarget;
            candidates = matchingLinkType;
        }
        string[] languages = ParsePreferences(acceptLanguage);
        if (languages.Length > 0)
        {
            LinkTarget[] matchingLanguage = candidates
                .Where(target => target.Language is not null && languages.Any(language => LanguageMatches(language, target.Language)))
                .OrderBy(target => Array.FindIndex(languages, language => LanguageMatches(language, target.Language!)))
                .ToArray();
            if (matchingLanguage.Length == 0)
                return defaultTarget;
            candidates = matchingLanguage;
        }
        string[] mediaTypes = ParsePreferences(accept);
        if (mediaTypes.Length > 0)
        {
            LinkTarget[] matchingMediaType = candidates
                .Where(target => target.MediaType is not null && mediaTypes.Any(mediaType => MediaTypeMatches(mediaType, target.MediaType)))
                .OrderBy(target => Array.FindIndex(mediaTypes, mediaType => MediaTypeMatches(mediaType, target.MediaType!)))
                .ToArray();
            if (matchingMediaType.Length == 0)
                return defaultTarget;
            candidates = matchingMediaType;
        }
        LinkTarget[] remaining = candidates
            .OrderBy(target => target.Language is null ? 0 : 1)
            .ThenBy(target => target.Language, StringComparer.OrdinalIgnoreCase)
            .ThenBy(target => target.MediaType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(target => target.Url, StringComparer.Ordinal)
            .ToArray();
        return remaining.FirstOrDefault(target => target.IsDefault)
            ?? (remaining.Length > 0 && !string.IsNullOrWhiteSpace(linkType) ? remaining[0] : null)
            ?? (remaining.Length == 1 ? remaining[0] : defaultTarget);
    }
    private static string[] ParsePreferences(string? header) => string.IsNullOrWhiteSpace(header)
        ? []
        : header.Split(',')
            .Select((part, position) =>
            {
                string[] segments = part.Split(';', StringSplitOptions.TrimEntries);
                double quality = 1;
                foreach (string parameter in segments.Skip(1))
                {
                    if (parameter.StartsWith("q=", StringComparison.OrdinalIgnoreCase) &&
                        double.TryParse(parameter[2..], System.Globalization.NumberStyles.AllowDecimalPoint,
                            System.Globalization.CultureInfo.InvariantCulture, out double parsed))
                        quality = parsed;
                }
                return new { Value = segments[0].Trim(), Quality = quality, Position = position };
            })
            .Where(item => item.Value.Length > 0 && item.Quality > 0)
            .OrderByDescending(item => item.Quality)
            .ThenBy(item => item.Position)
            .Select(item => item.Value)
            .ToArray();

    private static bool LanguageMatches(string requested, string available) =>
        requested == "*" ||
        string.Equals(requested, available, StringComparison.OrdinalIgnoreCase) ||
        requested.StartsWith(available + "-", StringComparison.OrdinalIgnoreCase);
    private static bool MediaTypeMatches(string requested, string available)
    {
        if (requested == "*/*" || string.Equals(requested, available, StringComparison.OrdinalIgnoreCase))
            return true;
        int slash = requested.IndexOf('/');
        return slash > 0 && requested.EndsWith("/*", StringComparison.Ordinal) &&
            available.StartsWith(requested[..slash] + "/", StringComparison.OrdinalIgnoreCase);
    }
}