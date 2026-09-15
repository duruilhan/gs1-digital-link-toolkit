using System.Numerics;

namespace Gs1.DigitalLink;

public static class Gs1CompressedDigitalLinkParser
{
    private const string Safe64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    private const string HexAlphabet = "0123456789ABCDEF";

    private static readonly IReadOnlyDictionary<string, string[]> Optimisations =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["0A"] = ["01", "22"],
            ["0B"] = ["01", "10"],
            ["0C"] = ["01", "21"],
            ["0D"] = ["01", "17"],
            ["1A"] = ["01", "10", "21", "17"],
            ["1C"] = ["01", "11"],
            ["2D"] = ["01", "3103"]
        };

    public static bool TryParse(string? uri, out IReadOnlyList<Gs1Element> elements)
    {
        elements = [];
        if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed) ||
            (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
            return false;

        string compressed = parsed.AbsolutePath.Trim('/');
        if (compressed.Length == 0 || compressed.Contains('/') || compressed.Any(character => !Safe64Alphabet.Contains(character)))
            return false;

        try
        {
            var reader = new BitReader(ToBits(compressed));
            var decoded = new List<Gs1Element>();
            while (reader.Remaining > 8)
            {
                string header = reader.ReadHexDigit().ToString() + reader.ReadHexDigit();
                if (Optimisations.TryGetValue(header, out string[]? sequence))
                {
                    foreach (string optimisedCode in sequence)
                        decoded.Add(new Gs1Element(optimisedCode, DecodeValue(optimisedCode, reader)));
                    continue;
                }

                if (!header.All(char.IsAsciiDigit))
                    return false;

                string code = header;
                if (!ApplicationIdentifierCatalog.TryGet(code, out _))
                {
                    code += reader.ReadHexDigit();
                    if (!ApplicationIdentifierCatalog.TryGet(code, out _))
                        code += reader.ReadHexDigit();
                }

                if (!ApplicationIdentifierCatalog.TryGet(code, out _))
                    return false;
                decoded.Add(new Gs1Element(code, DecodeValue(code, reader)));
            }

            if (decoded.Count == 0 || decoded.Any(element =>
                    !ApplicationIdentifierValidator.IsValid(element.ApplicationIdentifier, element.Value)))
                return false;

            _ = Gs1DigitalLinkBuilder.Build(decoded);
            elements = decoded;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return false;
        }
    }

    private static string DecodeValue(string code, BitReader reader)
    {
        ApplicationIdentifierCatalog.TryGet(code, out ApplicationIdentifierDefinition? definition);
        if (definition is null)
            throw new InvalidOperationException($"Unsupported AI {code}.");

        if (definition.IsNumeric)
        {
            int length = definition.IsFixedLength
                ? definition.MaxLength
                : reader.ReadInt(NumberOfLengthBits(definition.MaxLength));
            if (length < definition.MinLength || length > definition.MaxLength)
                throw new InvalidOperationException($"Invalid length for AI {code}.");
            return reader.ReadBigInteger(NumberOfValueBits(length)).ToString().PadLeft(length, '0');
        }

        int encoding = reader.ReadInt(3);
        int characterCount = definition.IsFixedLength
            ? definition.MaxLength
            : reader.ReadInt(NumberOfLengthBits(definition.MaxLength));
        if (characterCount < definition.MinLength || characterCount > definition.MaxLength)
            throw new InvalidOperationException($"Invalid length for AI {code}.");
        return encoding switch
        {
            0 => reader.ReadBigInteger(NumberOfValueBits(characterCount)).ToString().PadLeft(characterCount, '0'),
            1 => reader.ReadCharacters(characterCount, 4, HexAlphabet).ToLowerInvariant(),
            2 => reader.ReadCharacters(characterCount, 4, HexAlphabet),
            3 => reader.ReadCharacters(characterCount, 6, Safe64Alphabet),
            4 => reader.ReadAscii(characterCount),
            _ => throw new InvalidOperationException("Unsupported compressed value encoding.")
        };
    }

    private static int NumberOfLengthBits(int maximumLength) =>
        (int)Math.Ceiling(Math.Log2(maximumLength) + 0.01);

    private static int NumberOfValueBits(int length) =>
        (int)Math.Ceiling(length * Math.Log2(10) + 0.01);

    private static string ToBits(string value) => string.Concat(value.Select(character =>
    {
        int index = Safe64Alphabet.IndexOf(character);
        if (index < 0)
            throw new InvalidOperationException("Invalid safe-base64 character.");
        return Convert.ToString(index, 2).PadLeft(6, '0');
    }));

    private sealed class BitReader(string bits)
    {
        private int position;
        internal int Remaining => bits.Length - position;

        internal char ReadHexDigit() => HexAlphabet[ReadInt(4)];

        internal int ReadInt(int count)
        {
            if (count < 0 || Remaining < count)
                throw new InvalidOperationException("The compressed value ended unexpectedly.");
            int value = 0;
            for (int index = 0; index < count; index++)
                value = (value << 1) | (bits[position++] - '0');
            return value;
        }

        internal BigInteger ReadBigInteger(int count)
        {
            BigInteger value = BigInteger.Zero;
            for (int index = 0; index < count; index++)
                value = (value << 1) | ReadInt(1);
            return value;
        }

        internal string ReadCharacters(int count, int bitsPerCharacter, string alphabet) =>
            new(Enumerable.Range(0, count).Select(_ => alphabet[ReadInt(bitsPerCharacter)]).ToArray());

        internal string ReadAscii(int count) =>
            new(Enumerable.Range(0, count).Select(_ => (char)ReadInt(7)).ToArray());
    }
}
