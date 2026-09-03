using System.Text.Json.Serialization;

namespace Gs1.DigitalLink
{
    [JsonConverter(typeof(JsonStringEnumConverter<ApplicationIdentifierRole>))]
    public enum ApplicationIdentifierRole
    {
        Unknown,
        PrimaryKey,
        KeyQualifier,
        DataAttribute
    }
}
