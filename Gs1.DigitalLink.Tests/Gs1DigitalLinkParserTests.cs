using Gs1.DigitalLink;
using Gs1.DigitalLink.TestSupport;
namespace Gs1.DigitalLink.Tests
{
    public class Gs1DigitalLinkParserTests
    {
        private static string Describe(IReadOnlyList<Gs1Element> elements) =>
            string.Join("|", elements.Select(e => $"{e.ApplicationIdentifier}={e.Value}"));
        [Theory]
        [InlineData("https://id.gs1.org/01/08690504080008", "01=08690504080008")]
        [InlineData("https://id.gs1.org/01/08690504080008/10/LOT123/21/SER1", "01=08690504080008|10=LOT123|21=SER1")]
        [InlineData("https://id.gs1.org/01/08690504080008?17=261231", "01=08690504080008|17=261231")]
        [InlineData("https://id.gs1.org/01/08690504080008/10/LOT123?17=261231&3103=000189", "01=08690504080008|10=LOT123|17=261231|3103=000189")]
        [InlineData("https://id.gs1.org/01/08690504080008/10/A%2FB", "01=08690504080008|10=A/B")]
        [InlineData("https://id.gs1.org/01/08690504080008/10/50%25", "01=08690504080008|10=50%")]
        [InlineData("https://id.gs1.org/01/08690504080008/10/%2E", "01=08690504080008|10=.")]
        [InlineData("https://example.com/dl/01/08690504080008", "01=08690504080008")]
        [InlineData("https://id.gs1.org/414/8690123456789", "414=8690123456789")]
        public void Parse_WithValidUrl_ReturnsExpectedElements(string input, string expected)
        {
            Assert.Equal(expected, Describe(Gs1DigitalLinkParser.Parse(input)));
            Assert.True(Gs1DigitalLinkParser.TryParse(input, out var result));
            Assert.Equal(expected, Describe(result));
        }
        [Theory]
        [InlineData("https://id.gs1.org/")]
        [InlineData("https://id.gs1.org/10/LOT123")]
        [InlineData("https://id.gs1.org/01/8690504080008")]
        [InlineData("https://id.gs1.org/01/08690504080008/21/SER1/10/LOT123")]
        [InlineData("https://id.gs1.org/01/08690504080008/99/ABC")]
        [InlineData("https://id.gs1.org/01/08690504080008/17/261231")]
        [InlineData("https://id.gs1.org/414/8690123456789/10/LOT123")]
        [InlineData("https://id.gs1.org/01/08690504080008?10=LOT123")]
        [InlineData("https://id.gs1.org/01/08690504080008?17=261231&17=261231")]
        [InlineData("https://id.gs1.org/01/08690504080008?")]
        [InlineData("https://id.gs1.org/01/08690504080008?17")]
        [InlineData("https://id.gs1.org/01/08690504080008?17=")]
        [InlineData("https://id.gs1.org/01/08690504080008/10/A%2")]
        [InlineData("ftp://id.gs1.org/01/08690504080008")]
        [InlineData("not-a-url")]
        [InlineData("https://id.gs1.org/01/08690504080008#fragment")]
        [InlineData("")]
        [InlineData(null)]
        public void TryParse_WithInvalidUrl_ReturnsFalseAndEmptyResult(string? input)
        {
            Assert.False(Gs1DigitalLinkParser.TryParse(input, out var result));
            Assert.Empty(result);
        }
        [Fact]
        public void Parse_WithInvalidUrl_ThrowsParseException()
        {
            var exception = Assert.Throws<Gs1ParseException>(() =>
                Gs1DigitalLinkParser.Parse("https://id.gs1.org/01/08690504080008/21/SER1/10/LOT123"));
            Assert.True(exception.Position >= 0);
            Assert.Throws<ArgumentNullException>(() => Gs1DigitalLinkParser.Parse(null!));
        }
        [Fact]
        public void BuildThenParse_WithOneThousandSeededInputs_PreservesCanonicalMeaning()
        {
            const int seed = 20260904;
            var random = new Random(seed);
            for (int iteration = 0; iteration < 1_000; iteration++)
            {
                var input = ValidElementGenerator.Generate(random);
                string url = Gs1DigitalLinkBuilder.Build(input);
                Assert.True(Gs1DigitalLinkParser.TryParse(url, out var parsed),
                    $"Seed {seed}, iteration {iteration}, URL {url}");
                Assert.Equal(url, Gs1DigitalLinkBuilder.Build(parsed));
                Assert.Equal(ValidElementGenerator.Canonicalize(input), parsed);
            }
        }
        [Fact]
        public void ParseBuildEquality_IsNotTheCorrectProperty_WhenInputOrderIsNonCanonical()
        {
            Gs1Element[] input = [new("21", "SER1"), new("01", "08690504080008"), new("10", "LOT123")];
            var parsed = Gs1DigitalLinkParser.Parse(Gs1DigitalLinkBuilder.Build(input));
            Assert.NotEqual(input, parsed);
            Assert.Equal(ValidElementGenerator.Canonicalize(input), parsed);
            Assert.Equal(Gs1DigitalLinkBuilder.Build(input), Gs1DigitalLinkBuilder.Build(parsed));
        }

    }
}