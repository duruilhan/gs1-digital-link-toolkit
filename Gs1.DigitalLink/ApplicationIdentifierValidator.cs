namespace Gs1.DigitalLink;
public static class ApplicationIdentifierValidator
{
    public static bool IsValid(string? aiCode, string? value)
    {
        if (value is null || !ApplicationIdentifierCatalog.TryGet(aiCode, out var definition))
        {
            return false;
        }
        return definition!.Format switch
        {
            ['N', .. var length] when int.TryParse(length, out int exactLength) =>
                value.Length == exactLength && value.All(char.IsDigit),
            ['X', '.', '.', .. var maximum] when int.TryParse(maximum, out int maximumLength) =>
                value.Length is > 0 && value.Length <= maximumLength && value.All(IsTemporaryCset82Character),
            _ => false
        };
    }
    private static bool IsTemporaryCset82Character(char character) =>
        char.IsAsciiLetterOrDigit(character) || character == '-';
}