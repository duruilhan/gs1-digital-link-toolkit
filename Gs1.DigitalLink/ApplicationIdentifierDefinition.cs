using System.Text.Json.Serialization;
namespace Gs1.DigitalLink
{
public sealed record ApplicationIdentifierDefinition
{
    [JsonConstructor]
    public ApplicationIdentifierDefinition(
        string code,
        string title,
        string format,
        bool isFixedLength,
        bool hasCheckDigit)
    {
        Code = code;
        Title = title;
        Format = format;
        IsFixedLength = isFixedLength;
        HasCheckDigit = hasCheckDigit;
        (IsNumeric, MinLength, MaxLength) = ParseFormat(format);
    }
    public string Code { get; }
    public string Title { get; }
    public string Format { get; }
    public bool IsFixedLength { get; }
    public bool HasCheckDigit { get; }
    public bool IsNumeric { get; }
    public int MinLength { get; }
    public int MaxLength { get; }
    private static (bool IsNumeric, int MinLength, int MaxLength) ParseFormat(string format)
    {
        if (format.Length > 1 &&
            format[0] == 'N' &&
            int.TryParse(format[1..], out int exactLength) &&
            exactLength > 0)
        {
            return (true, exactLength, exactLength);
        }
        if (format.StartsWith("X..", StringComparison.Ordinal) &&
            int.TryParse(format[3..], out int maximumLength) &&
            maximumLength > 0)
        {
            return (false, 1, maximumLength);
        }
        throw new InvalidDataException($"Unsupported Application Identifier format: '{format}'.");
    }
}
}
