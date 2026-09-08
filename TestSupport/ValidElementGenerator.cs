using Gs1.DigitalLink;

namespace Gs1.DigitalLink.TestSupport;

internal static class ValidElementGenerator
{
    internal static Gs1Element[] Generate(Random random)
    {
        var elements = new List<Gs1Element> { new("01", WithCheckDigit(RandomDigits(random, 13))) };
        if (random.Next(2) == 1) elements.Add(new("22", RandomText(random, 1, 20)));
        if (random.Next(2) == 1) elements.Add(new("10", RandomText(random, 1, 20)));
        if (random.Next(2) == 1) elements.Add(new("21", RandomText(random, 1, 20)));
        if (random.Next(2) == 1) elements.Add(new("11", RandomDate(random)));
        if (random.Next(2) == 1) elements.Add(new("17", RandomDate(random)));
        if (random.Next(2) == 1) elements.Add(new("240", RandomText(random, 1, 30)));
        if (random.Next(2) == 1) elements.Add(new("3103", RandomDigits(random, 6)));
        return elements.OrderBy(_ => random.Next()).ToArray();
    }

    internal static IReadOnlyList<Gs1Element> Canonicalize(IEnumerable<Gs1Element> input) =>
        input.OrderBy(element => element.ApplicationIdentifier == "01" ? 0 :
                element.ApplicationIdentifier == "22" ? 1 :
                element.ApplicationIdentifier == "10" ? 2 :
                element.ApplicationIdentifier == "21" ? 3 : 4)
            .ThenBy(element => element.ApplicationIdentifier, StringComparer.Ordinal).ToArray();

    private static string RandomDigits(Random random, int length) =>
        new(Enumerable.Range(0, length).Select(_ => (char)('0' + random.Next(10))).ToArray());

    private static string WithCheckDigit(string digits) => digits + CheckDigitCalculator.Calculate(digits);

    private static string RandomDate(Random random)
    {
        DateTime first = new(2020, 1, 1);
        DateTime date = first.AddDays(random.Next((new DateTime(2036, 1, 1) - first).Days));
        return date.ToString("yyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string RandomText(Random random, int minimum, int maximum)
    {
        const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!%&+,-./:;<=>?_";
        int length = random.Next(minimum, maximum + 1);
        return new string(Enumerable.Range(0, length).Select(_ => characters[random.Next(characters.Length)]).ToArray());
    }
}
