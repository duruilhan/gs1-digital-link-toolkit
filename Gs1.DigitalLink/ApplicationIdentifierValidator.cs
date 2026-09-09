namespace Gs1.DigitalLink
{
public static class ApplicationIdentifierValidator
{
    private const string Cset82Punctuation = "!\"%&'()*+,-./:;<=>?_";
    public static bool IsValid(string? aiCode, string? value)
    {
        if (value is null || !ApplicationIdentifierCatalog.TryGet(aiCode, out var definition))
        {
            return false;
        }
        bool hasValidLength = value.Length >= definition!.MinLength &&
                              value.Length <= definition.MaxLength;
        bool hasValidCharacters = definition.IsNumeric
            ? value.All(char.IsDigit)
            : value.All(IsCset82Character);
        if (!hasValidLength || !hasValidCharacters)
        {
            return false;
        }
        if ((aiCode == "11" || aiCode == "17") && !IsValidGs1Date(value))
        {
            return false;
        }
        return !definition.HasCheckDigit || CheckDigitCalculator.IsValid(value);
    }

    private static bool IsValidGs1Date(string value)
    {
        if (!int.TryParse(value.AsSpan(0, 2), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int shortYear) ||
            !int.TryParse(value.AsSpan(2, 2), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int month) ||
            !int.TryParse(value.AsSpan(4, 2), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int day))
        {
            return false;
        }

        int year = 2000 + shortYear;

        if (month is < 1 or > 12)
        {
            return false;
        }

        // Legacy GS1 data may use 00 to mean the final day of the specified month.
        return day == 0 || day <= DateTime.DaysInMonth(year, month);
    }

    private static bool IsCset82Character(char character) =>
        char.IsAsciiLetterOrDigit(character) || Cset82Punctuation.Contains(character);
}
}
