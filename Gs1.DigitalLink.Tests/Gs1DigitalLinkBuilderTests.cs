using Gs1.DigitalLink;
namespace Gs1.DigitalLink.Tests
{
    public class Gs1DigitalLinkBuilderTests
    {
        private static Gs1Element[] Elements(string input) => input.Length == 0 ? [] :
            input.Split('|').Select(pair => pair.Split('=', 2))
                .Select(pair => new Gs1Element(pair[0], pair[1])).ToArray();
        [Theory]
        [InlineData("01=08690504080008", "/01/08690504080008")]
        [InlineData("01=08690504080008|10=LOT123", "/01/08690504080008/10/LOT123")]
        [InlineData("01=08690504080008|10=LOT123|21=SER1", "/01/08690504080008/10/LOT123/21/SER1")]
        [InlineData("01=08690504080008|21=SER1|10=LOT123", "/01/08690504080008/10/LOT123/21/SER1")]
        [InlineData("01=08690504080008|22=VAR1|10=LOT123|21=SER1", "/01/08690504080008/22/VAR1/10/LOT123/21/SER1")]
        [InlineData("21=SER1|10=LOT123|22=VAR1|01=08690504080008", "/01/08690504080008/22/VAR1/10/LOT123/21/SER1")]
        [InlineData("01=08690504080008|17=261231", "/01/08690504080008?17=261231")]
        [InlineData("01=08690504080008|10=LOT123|17=261231|3103=000189", "/01/08690504080008/10/LOT123?17=261231&3103=000189")]
        [InlineData("414=8690123456789", "/414/8690123456789")]
        [InlineData("00=869012345123456784", "/00/869012345123456784")]
        [InlineData("8018=869012345123456784", "/8018/869012345123456784")]
        [InlineData("01=08690504080008|10=A/B", "/01/08690504080008/10/A%2FB")]
        [InlineData("01=08690504080008|10=50%", "/01/08690504080008/10/50%25")]
        [InlineData("01=08690504080008|10=A?&+=:;", "/01/08690504080008/10/A%3F%26%2B%3D%3A%3B")]
        [InlineData("01=08690504080008|240=A/B?%&+=:;", "/01/08690504080008?240=A%2FB%3F%25%26%2B%3D%3A%3B")]
        [InlineData("01=08690504080008|3103=000189|240=ABC|17=261231|11=260101", "/01/08690504080008?11=260101&17=261231&240=ABC&3103=000189")]
        [InlineData("01=08690504080008|21=SER1", "/01/08690504080008/21/SER1")]
        public void Build_WithValidElements_ReturnsExpectedUrl(string input, string path)
        {
            Assert.Equal("https://id.gs1.org" + path, Gs1DigitalLinkBuilder.Build(Elements(input)));
        }
        [Theory]
        [InlineData("")]
        [InlineData("10=LOT123")]
        [InlineData("01=08690504080008|414=8690123456789")]
        [InlineData("414=8690123456789|10=LOT123")]
        [InlineData("00=869012345123456784|21=SER1")]
        [InlineData("8018=869012345123456784|22=VAR1")]
        [InlineData("01=08690504080009")]
        [InlineData("01=08690504080008|99=ABC")]
        [InlineData("01=08690504080008|10=")]
        [InlineData("01=08690504080008|17=2612A1")]
        [InlineData("01=08690504080008|10=A B")]
        [InlineData("01=08690504080008|01=08690504080008")]
        [InlineData("01=08690504080008|10=A|10=B")]
        [InlineData("01=08690504080008|17=261231|17=261231")]
        public void Build_WithInvalidElements_ThrowsArgumentException(string input)
        {
            Assert.Throws<ArgumentException>(() => Gs1DigitalLinkBuilder.Build(Elements(input)));
        }
        [Fact]
        public void Build_WithNullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Gs1DigitalLinkBuilder.Build(null!));
            Assert.Throws<ArgumentException>(() => Gs1DigitalLinkBuilder.Build([null!]));
            Assert.Throws<ArgumentException>(() => Gs1DigitalLinkBuilder.Build([new("01", null!)]));
        }
        [Theory]
        [InlineData("https://example.org", "https://example.org/01/08690504080008")]
        [InlineData("https://example.org/", "https://example.org/01/08690504080008")]
        [InlineData("https://example.org/id/", "https://example.org/id/01/08690504080008")]
        public void Build_WithCustomBaseAddress_ReturnsExpectedUrl(string root, string expected)
        {
            Assert.Equal(expected, Gs1DigitalLinkBuilder.Build(Elements("01=08690504080008"), root));
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("relative/path")]
        [InlineData("ftp://example.org")]
        [InlineData("https://example.org/?a=b")]
        [InlineData("https://example.org/#part")]
        [InlineData("https://user:password@example.org")]
        public void Build_WithInvalidBaseAddress_ThrowsArgumentException(string? root)
        {
            Assert.Throws<ArgumentException>(() => Gs1DigitalLinkBuilder.Build(Elements("01=08690504080008"), root!));
        }
        [Fact]
        public void Build_DoesNotReorderOriginalInput()
        {
            var input = Elements("21=SER1|01=08690504080008|10=LOT123");
            var original = input.ToArray();
            Gs1DigitalLinkBuilder.Build(input);
            Assert.Equal(original, input);
        }
        [Fact]
        public void Build_FromRawParser_ReturnsDigitalLink()
        {
            string raw = "010869050408000810LOT123\u001D17261231";
            Assert.Equal("https://id.gs1.org/01/08690504080008/10/LOT123?17=261231",
                Gs1DigitalLinkBuilder.Build(Gs1RawElementStringParser.Parse(raw)));
        }
        [Fact]
        public void Build_FromParenthesizedParser_ReturnsDigitalLink()
        {
            Assert.Equal("https://id.gs1.org/01/08690504080008/10/LOT123",
                Gs1DigitalLinkBuilder.Build(Gs1ElementStringParser.Parse("(01)08690504080008(10)LOT123")));
        }
    }
}
