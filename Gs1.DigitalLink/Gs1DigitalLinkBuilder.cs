namespace Gs1.DigitalLink
{
    /// <summary>Builds uncompressed Digital Link URLs for the supported AI catalog.</summary>
    public static class Gs1DigitalLinkBuilder
    {
        /// <summary>Validates elements and builds a URL without modifying the input.</summary>
        /// <exception cref="ArgumentException">The input or base address is invalid.</exception>
        public static string Build(IEnumerable<Gs1Element> elements,
            string baseAddress = "https://id.gs1.org")
        {
            ArgumentNullException.ThrowIfNull(elements);
            if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var root) ||
                (root.Scheme != Uri.UriSchemeHttps && root.Scheme != Uri.UriSchemeHttp) ||
                root.Query.Length != 0 || root.Fragment.Length != 0 || root.UserInfo.Length != 0)
                throw new ArgumentException("Base address must be an absolute HTTP(S) URL without credentials, query or fragment.", nameof(baseAddress))
            var entries = new List<(Gs1Element Element, ApplicationIdentifierDefinition Definition)>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var element in elements)
            {
                if (element is null || !ApplicationIdentifierValidator.IsValid(element.ApplicationIdentifier, element.Value))
                    throw new ArgumentException("Every element must have a known AI and a valid value.", nameof(elements));
                if (!seen.Add(element.ApplicationIdentifier))
                    throw new ArgumentException($"Duplicate AI: {element.ApplicationIdentifier}.", nameof(elements));
                ApplicationIdentifierCatalog.TryGet(element.ApplicationIdentifier, out var definition);
                entries.Add((element, definition!));
            }

            var primaryKeys = entries.Where(e => e.Definition.Role == ApplicationIdentifierRole.PrimaryKey).ToList();
            if (primaryKeys.Count != 1)
                throw new ArgumentException("Exactly one primary key is required.", nameof(elements));
            var primary = primaryKeys[0];
            var qualifiers = entries.Where(e => e.Definition.Role == ApplicationIdentifierRole.KeyQualifier)
                .OrderBy(e => e.Definition.QualifierOrder).ToList();
            if (qualifiers.Any(e => e.Definition.QualifierFor != primary.Element.ApplicationIdentifier))
                throw new ArgumentException("A qualifier cannot be used with this primary key.", nameof(elements));

            static string EscapePathValue(string value) => value switch
            {
                "." => "%2E",
                ".." => "%2E%2E",
                _ => Uri.EscapeDataString(value)
            };
            static string PathPart(Gs1Element element) =>
                $"/{Uri.EscapeDataString(element.ApplicationIdentifier)}/{EscapePathValue(element.Value)}";
            string url = root.AbsoluteUri.TrimEnd('/') + PathPart(primary.Element) +
                string.Concat(qualifiers.Select(e => PathPart(e.Element)));
            var attributes = entries.Where(e => e.Definition.Role == ApplicationIdentifierRole.DataAttribute)
                .OrderBy(e => e.Element.ApplicationIdentifier, StringComparer.Ordinal)
                .Select(e => $"{Uri.EscapeDataString(e.Element.ApplicationIdentifier)}={Uri.EscapeDataString(e.Element.Value)}");
            string query = string.Join("&", attributes);
            return query.Length == 0 ? url : url + "?" + query;
        }
    }
}