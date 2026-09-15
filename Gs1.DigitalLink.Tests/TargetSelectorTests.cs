using Gs1.DigitalLink.Resolver;
namespace Gs1.DigitalLink.Tests;
public sealed class TargetSelectorTests
{
    [Fact]
    public void Select_UsesQualityValuesInsteadOfHeaderOrder()
    {
        LinkTarget turkish = Target("gs1:pip", "tr", "text/html");
        LinkTarget english = Target("gs1:pip", "en", "text/html");
        LinkTarget? selected = TargetSelector.Select(
            [turkish, english], "gs1:pip", "tr;q=0.4, en;q=0.9", "text/html");
        Assert.Same(english, selected);
    }
    [Fact]
    public void Select_ExplicitLinkTypeWithoutLanguage_DoesNotReturnAnotherLinkType()
    {
        LinkTarget defaultTarget = Target("gs1:defaultLink", null, "text/html", true);
        LinkTarget turkish = Target("gs1:pip", "tr", "text/html");
        LinkTarget english = Target("gs1:pip", "en", "text/html");
        LinkTarget? selected = TargetSelector.Select(
            [defaultTarget, turkish, english], "gs1:pip", null, null);
        Assert.Same(english, selected);
    }
    [Fact]
    public void Select_UnsupportedLanguageFallsBackToDefault_AndAmbiguityWithoutDefaultReturnsNull()
    {
        LinkTarget defaultTarget = Target("gs1:defaultLink", null, "text/html", true);
        LinkTarget turkish = Target("gs1:pip", "tr", "text/html");
        LinkTarget english = Target("gs1:pip", "en", "application/json");
        Assert.Same(defaultTarget,
            TargetSelector.Select([defaultTarget, turkish, english], "gs1:pip", "de", "text/html"));
        Assert.Null(TargetSelector.Select([turkish, english], null, null, null));
    }
    private static LinkTarget Target(string linkType, string? language, string? mediaType, bool isDefault = false) => new()
    {
        Id = Guid.NewGuid(),
        LinkType = linkType,
        Url = "https://example.com/" + (language ?? "default"),
        Language = language,
        MediaType = mediaType,
        IsDefault = isDefault
    };
}