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
        bool hasCheckDigit,
        ApplicationIdentifierRole role,
        int? qualifierOrder = null,
        string? qualifierFor = null)
    {
        Code = code;
        Title = title;
        Format = format;
        IsFixedLength = isFixedLength;
        HasCheckDigit = hasCheckDigit;
        if (!Enum.IsDefined(role) || role == ApplicationIdentifierRole.Unknown)
            throw new InvalidDataException("An AI must have a Digital Link role.");
        if (role == ApplicationIdentifierRole.KeyQualifier &&
            (qualifierOrder is null or <= 0 || string.IsNullOrEmpty(qualifierFor)))
            throw new InvalidDataException("A qualifier must specify its primary key and order.");
        Role = role;
        QualifierOrder = qualifierOrder;
        QualifierFor = qualifierFor;
        (IsNumeric, MinLength, MaxLength) = ParseFormat(format);
    }
    public string Code { get; }
    public string Title { get; }
    public string Format { get; }
    public bool IsFixedLength { get; }
    public bool HasCheckDigit { get; }
    public ApplicationIdentifierRole Role { get; }
    public int? QualifierOrder { get; }
    public string? QualifierFor { get; }
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
