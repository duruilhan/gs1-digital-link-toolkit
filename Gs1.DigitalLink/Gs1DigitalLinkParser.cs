namespace Gs1.DigitalLink
{
    /// <summary>Parses uncompressed GS1 Digital Link URLs into AI/value elements.</summary>
    public static class Gs1DigitalLinkParser
    {
        public static bool TryParse(string? input, out IReadOnlyList<Gs1Element> result)
        {
            result = [];
            if (input is null)
                return false;

            try
            {
                result = ParseCore(input);
                return true;
            }
            catch (Exception exception) when (exception is FormatException or ArgumentException or UriFormatException)
            {
                result = [];
                return false;
            }
        }

        public static IReadOnlyList<Gs1Element> Parse(string input)
        {
            ArgumentNullException.ThrowIfNull(input);
            return ParseCore(input);
        }

        private static IReadOnlyList<Gs1Element> ParseCore(string input)
        {
            ValidatePercentEncoding(input);
            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
                uri.UserInfo.Length != 0 || uri.Fragment.Length != 0)
                throw Error("Input must be an absolute HTTP(S) URL without credentials or fragment", 0);

            int authorityEnd = input.IndexOf('/', input.IndexOf("://", StringComparison.Ordinal) + 3);
            int queryStart = input.IndexOf('?');
            int pathEnd = queryStart >= 0 ? queryStart : input.Length;
            string rawPath = authorityEnd >= 0 && authorityEnd < pathEnd
                ? input[authorityEnd..pathEnd]
                : string.Empty;
            string[] path = rawPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            int primaryIndex = Array.FindIndex(path, encoded =>
            {
                string code = Decode(encoded, input);
                return ApplicationIdentifierCatalog.TryGet(code, out var definition) &&
                    definition!.Role == ApplicationIdentifierRole.PrimaryKey;
            });
            if (primaryIndex < 0 || primaryIndex + 1 >= path.Length)
                throw Error("The path does not contain a primary key and value", 0);
            if ((path.Length - primaryIndex) % 2 != 0)
                throw Error("Every path AI must have a value", input.Length);

            var result = new List<Gs1Element>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int previousQualifierOrder = 0;
            string primaryCode = Decode(path[primaryIndex], input);

            for (int i = primaryIndex; i < path.Length; i += 2)
            {
                string code = Decode(path[i], input);
                string value = Decode(path[i + 1], input);
                if (!ApplicationIdentifierCatalog.TryGet(code, out var definition))
                    throw Error($"Unknown path AI '{code}'", FindPosition(input, path[i]));
                if (i == primaryIndex)
                {
                    if (definition!.Role != ApplicationIdentifierRole.PrimaryKey)
                        throw Error("The first AI is not a primary key", FindPosition(input, path[i]));
                }
                else
                {
                    if (definition!.Role != ApplicationIdentifierRole.KeyQualifier ||
                        definition.QualifierFor != primaryCode)
                        throw Error($"AI '{code}' is not a qualifier for '{primaryCode}'", FindPosition(input, path[i]));
                    if (definition.QualifierOrder <= previousQualifierOrder)
                        throw Error("Key qualifiers are not in canonical order", FindPosition(input, path[i]));
                    previousQualifierOrder = definition.QualifierOrder!.Value;
                }
                AddValidated(result, seen, code, value, input, path[i + 1]);
            }

            if (queryStart >= 0)
            {
                string query = input[(queryStart + 1)..];
                if (query.Length == 0)
                    throw Error("The query string is empty", input.Length - 1);
                foreach (string pair in query.Split('&'))
                {
                    int equals = pair.IndexOf('=');
                    if (equals <= 0 || equals != pair.LastIndexOf('='))
                        throw Error("Every query parameter must contain one AI and value", FindPosition(input, pair));
                    string encodedCode = pair[..equals];
                    string encodedValue = pair[(equals + 1)..];
                    string code = Decode(encodedCode, input);
                    string value = Decode(encodedValue, input);
                    if (!ApplicationIdentifierCatalog.TryGet(code, out var definition) ||
                        definition!.Role != ApplicationIdentifierRole.DataAttribute)
                        throw Error($"AI '{code}' is not a data attribute", FindPosition(input, encodedCode));
                    AddValidated(result, seen, code, value, input, encodedValue);
                }
            }
            return result;
        }

        private static void AddValidated(List<Gs1Element> result, HashSet<string> seen,
            string code, string value, string input, string encodedValue)
        {
            if (!seen.Add(code))
                throw Error($"Duplicate AI '{code}'", FindPosition(input, encodedValue));
            if (!ApplicationIdentifierValidator.IsValid(code, value))
                throw Error($"Invalid value for AI '{code}'", FindPosition(input, encodedValue));
            result.Add(new Gs1Element(code, value));
        }

        private static string Decode(string value, string input)
        {
            return Uri.UnescapeDataString(value);
        }

        private static void ValidatePercentEncoding(string input)
        {
            for (int i = 0; i < input.Length; i++)
                if (input[i] == '%' && (i + 2 >= input.Length ||
                    !Uri.IsHexDigit(input[i + 1]) || !Uri.IsHexDigit(input[i + 2])))
                    throw Error("Invalid percent encoding", i);
        }

        private static int FindPosition(string input, string value) =>
            Math.Max(0, input.IndexOf(value, StringComparison.Ordinal));

        private static Gs1ParseException Error(string message, int position) => new(message, position);
    }
}
