using Gs1.DigitalLink;
namespace Gs1.DigitalLink.Tests
{
    public class OfficialGs1CompatibilityTests
    {
        private const string RawExample = "3103000189010541234500001339232172\u001D10ABC&+123";
        private const string DigitalLinkExample = "https://id.gs1.org/01/05412345000013/10/ABC%26%2B123?3103=000189&3923=2172";
        [Fact]
        public void OfficialExampleA_RawElementStringBuildsExpectedDigitalLink()
        {
            var elements = Gs1RawElementStringParser.Parse(RawExample);
            Assert.Equal(DigitalLinkExample, Gs1DigitalLinkBuilder.Build(elements));
        }
        [Fact]
        public void OfficialExampleA_DigitalLinkParsesToExpectedElements()
        {
            var elements = Gs1DigitalLinkParser.Parse(DigitalLinkExample);
            Assert.Equal("(01)05412345000013(10)ABC&+123(3103)000189(3923)2172", ToParenthesized(elements));
        }
        [Fact]
        public void OfficialExampleB_AliasesAndTwelveDigitGtinParseToExpectedElements()
        {
            const string input = "http://example.org/gtin/054123450013/lot/ABC%26%2B123?3103=000189&3923=2172";
            var elements = Gs1DigitalLinkParser.Parse(input);
            Assert.Equal("(01)00054123450013(10)ABC&+123(3103)000189(3923)2172", ToParenthesized(elements));
        }
        [Fact]
        public void TwelveDigitGtin_WithNumericAi_IsNormalizedToFourteenDigits()
        {
            var elements = Gs1DigitalLinkParser.Parse("https://id.gs1.org/01/054123450013");
            Assert.Equal("(01)00054123450013", ToParenthesized(elements));
        }
        [Theory]
        [InlineData("3923", "1")]
        [InlineData("3923", "123456789012345")]
        public void VariableLengthNumericFormat_AcceptsOneToFifteenDigits(string ai, string value)
        {
            Assert.True(ApplicationIdentifierValidator.IsValid(ai, value));
        }
        [Theory]
        [InlineData("")]
        [InlineData("1234567890123456")]
        [InlineData("12A")]
        public void VariableLengthNumericFormat_RejectsInvalidValues(string value)
        {
            Assert.False(ApplicationIdentifierValidator.IsValid("3923", value));
        }
        private static string ToParenthesized(IEnumerable<Gs1Element> elements) =>
            string.Concat(elements.Select(element => $"({element.ApplicationIdentifier}){element.Value}"));
    }
}