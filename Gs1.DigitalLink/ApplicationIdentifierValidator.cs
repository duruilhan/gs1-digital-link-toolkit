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
        return !definition.HasCheckDigit || CheckDigitCalculator.IsValid(value);
    }
    private static bool IsCset82Character(char character) =>
        char.IsAsciiLetterOrDigit(character) || Cset82Punctuation.Contains(character);
}
}