namespace Gs1.DigitalLink;
public sealed record ApplicationIdentifierDefinition(
    string Code,
    string Title,
    string Format,
    bool IsFixedLength,
    bool HasCheckDigit);