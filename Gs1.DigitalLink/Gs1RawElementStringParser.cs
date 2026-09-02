namespace Gs1.DigitalLink
{
    public static class Gs1RawElementStringParser
    {
        public const char GroupSeparator = '\u001D';
        public static bool TryParse(string? input, out IReadOnlyList<Gs1Element> result) =>
            TryParseCore(input, out result, out _);
        public static IReadOnlyList<Gs1Element> Parse(string input)
        {
            if (TryParseCore(input, out var result, out var error))
            {
                return result;
            }
            throw new Gs1ParseException(error.Message, error.Position);
        }
        private static bool TryParseCore(
            string? input,
            out IReadOnlyList<Gs1Element> result,
            out ParseError error)
        {
            var elements = new List<Gs1Element>();
            result = elements;
            if (string.IsNullOrEmpty(input))
            {
                return Fail("Input is empty", 0, out result, out error);
            }
            int position = 0;
            while (position < input.Length)
            {
                int aiPosition = position;
                if (!ApplicationIdentifierCatalog.TryMatchPrefix(input, position, out var definition))
                {
                    return Fail("Unknown Application Identifier", aiPosition, out result, out error);
                }
                string aiCode = definition!.Code;
                position += aiCode.Length;
                int valuePosition = position;
                string value;
                if (definition.IsFixedLength)
                {
                    int valueLength = definition.MaxLength;
                    if (position + valueLength > input.Length)
                    {
                        return Fail(
                            $"Value for Application Identifier '{aiCode}' is too short",
                            valuePosition,
                            out result,
                            out error);
                    }
                    value = input.Substring(position, valueLength);
                    position += valueLength;
                }
                else
                {
                    int separatorPosition = input.IndexOf(GroupSeparator, position);
                    int valueEnd = separatorPosition < 0 ? input.Length : separatorPosition;
                    value = input[position..valueEnd];
                    position = valueEnd;
                    if (separatorPosition >= 0)
                    {
                        position++;
                        if (position == input.Length)
                        {
                            return Fail("Input ends with a group separator", separatorPosition, out result, out error);
                        }
                    }
                }
                if (!ApplicationIdentifierValidator.IsValid(aiCode, value))
                {
                    return Fail(
                        $"Invalid value for Application Identifier '{aiCode}'",
                        valuePosition,
                        out result,
                        out error);
                }
                elements.Add(new Gs1Element(aiCode, value));
            }
            error = default;
            return true;
        }
        private static bool Fail(
            string message,
            int position,
            out IReadOnlyList<Gs1Element> result,
            out ParseError error)
        {
            result = [];
            error = new ParseError(message, position);
            return false;
        }
        private readonly record struct ParseError(string Message, int Position);
    }
}