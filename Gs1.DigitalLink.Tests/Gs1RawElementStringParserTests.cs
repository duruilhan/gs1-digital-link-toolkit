using Gs1.DigitalLink;
namespace Gs1.DigitalLink.Tests
{
    public class Gs1RawElementStringParserTests
    {
        private const char Gs = Gs1RawElementStringParser.GroupSeparator;
        public static TheoryData<string, Gs1Element[]> ValidRawElementStrings => new()
        {
            {
                "0108690504080008",
                [new("01", "08690504080008")]
            },
            {
                "010869050408000817261231",
                [new("01", "08690504080008"), new("17", "261231")]
            },
            {
                "010869050408000810LOT123",
                [new("01", "08690504080008"), new("10", "LOT123")]
            },
            {
                "010869050408000810LOT123" + Gs + "17261231",
                [new("01", "08690504080008"), new("10", "LOT123"), new("17", "261231")]
            },
            {
                "3103000189",
                [new("3103", "000189")]
            },
            {
                "10LOT123" + Gs + "0108690504080008",
                [new("10", "LOT123"), new("01", "08690504080008")]
            },
            {
                "8018869012345123456784" + Gs + "10LOT123",
                [new("8018", "869012345123456784"), new("10", "LOT123")]
            }
        };
        [Theory]
        [MemberData(nameof(ValidRawElementStrings))]
        public void Parse_WithValidRawInput_ReturnsExpectedElements(
            string input,
            Gs1Element[] expected)
        {
            IReadOnlyList<Gs1Element> result = Gs1RawElementStringParser.Parse(input);
            Assert.Equal(expected, result);
        }
        [Theory]
        [MemberData(nameof(ValidRawElementStrings))]
        public void TryParse_WithValidRawInput_ReturnsTrueAndExpectedElements(
            string input,
            Gs1Element[] expected)
        {
            bool parsed = Gs1RawElementStringParser.TryParse(input, out var result);
            Assert.True(parsed);
            Assert.Equal(expected, result);
        }
        [Theory]
        [InlineData("010869050408000")]
        [InlineData("9912345")]
        [InlineData("0108690504080009")]
        [InlineData("")]
        public void Parse_WithInvalidRawInput_Throws(string input)
        {
            Assert.Throws<Gs1ParseException>(() => Gs1RawElementStringParser.Parse(input));
        }
        [Fact]
        public void TryParse_WhenLaterElementIsInvalid_ReturnsFalseAndEmptyResult()
        {
            const string input = "01086905040800089912345";
            bool parsed = Gs1RawElementStringParser.TryParse(input, out var result);
            Assert.False(parsed);
            Assert.Empty(result);
        }
        [Fact]
        public void ParenthesizedTryParse_WhenLaterElementIsInvalid_ReturnsEmptyResult()
        {
            const string input = "(01)08690504080008(99)ABC";
            bool parsed = Gs1ElementStringParser.TryParse(input, out var result);
            Assert.False(parsed);
            Assert.Empty(result);
        }
        [Fact]
        public void Parse_WithoutSeparatorAfterVariableLengthValue_TreatsRemainderAsItsValue()
        {
            const string input = "10LOT12317261231";
            IReadOnlyList<Gs1Element> result = Gs1RawElementStringParser.Parse(input);
            Assert.Equal([new Gs1Element("10", "LOT12317261231")], result);
        }
    }
}