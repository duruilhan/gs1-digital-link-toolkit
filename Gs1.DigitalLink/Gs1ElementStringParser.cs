namespace Gs1.DigitalLink
{
    public static class Gs1ElementStringParser
    {
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
                error = new ParseError("Input is empty", 0);
                return false;
            }

            int position = 0;
            while (position < input.Length)
            {
                if (input[position] != '(')
                {
                    error = new ParseError("Expected an opening parenthesis", position);
                    return false;
                }

                int aiPosition = position + 1;
                int closingParenthesis = input.IndexOf(')', aiPosition);
                if (closingParenthesis < 0)
                {
                    error = new ParseError("Missing closing parenthesis for the Application Identifier", aiPosition);
                    return false;
                }

                string aiCode = input[aiPosition..closingParenthesis];
                if (!ApplicationIdentifierCatalog.TryGet(aiCode, out _))
                {
                    error = new ParseError($"Unknown Application Identifier '{aiCode}'", aiPosition);
                    return false;
                }

                int valuePosition = closingParenthesis + 1;
                int nextElement = input.IndexOf('(', valuePosition);
                int valueEnd = nextElement < 0 ? input.Length : nextElement;
                string value = input[valuePosition..valueEnd];

                if (!ApplicationIdentifierValidator.IsValid(aiCode, value))
                {
                    error = new ParseError($"Invalid value for Application Identifier '{aiCode}'", valuePosition);
                    return false;
                }

                elements.Add(new Gs1Element(aiCode, value));
                position = valueEnd;
            }

            error = default;
            return true;
        }

        private readonly record struct ParseError(string Message, int Position);
    }
}
