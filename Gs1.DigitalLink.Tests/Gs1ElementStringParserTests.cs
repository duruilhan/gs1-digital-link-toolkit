using Gs1.DigitalLink;

namespace Gs1.DigitalLink.Tests
{
    public class Gs1ElementStringParserTests
    {
        public static TheoryData<string, Gs1Element[]> ValidElementStrings => new()
        {
            {
                "(01)08690504080008",
                [new("01", "08690504080008")]
            },
            {
                "(01)08690504080008(10)LOT123",
                [new("01", "08690504080008"), new("10", "LOT123")]
            },
            {
                "(01)08690504080008(10)LOT123(17)261231",
                [new("01", "08690504080008"), new("10", "LOT123"), new("17", "261231")]
            },
            {
                "(17)261231(01)08690504080008",
                [new("17", "261231"), new("01", "08690504080008")]
            },
            {
                "(3103)000189",
                [new("3103", "000189")]
            },
            {
                "(240)ABC-123",
                [new("240", "ABC-123")]
            }
        };

        [Theory]
        [MemberData(nameof(ValidElementStrings))]
        public void Parse_WithValidInput_ReturnsElementsInInputOrder(
            string input,
            Gs1Element[] expected)
        {
            IReadOnlyList<Gs1Element> result = Gs1ElementStringParser.Parse(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(ValidElementStrings))]
        public void TryParse_WithValidInput_ReturnsTrueAndElements(
            string input,
            Gs1Element[] expected)
        {
            bool parsed = Gs1ElementStringParser.TryParse(input, out var result);

            Assert.True(parsed);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("01)08690504080008", 0)]
        [InlineData("(0108690504080008", 1)]
        [InlineData("(99)ABC", 1)]
        [InlineData("(01)8690504080008", 4)]
        [InlineData("(01)08690504080009", 4)]
        [InlineData("(01)", 4)]
        [InlineData("", 0)]
        public void Parse_WithInvalidInput_ThrowsWithExpectedPosition(
            string input,
            int expectedPosition)
        {
            Gs1ParseException exception = Assert.Throws<Gs1ParseException>(
                () => Gs1ElementStringParser.Parse(input));

            Assert.Equal(expectedPosition, exception.Position);
        }

        [Theory]
        [InlineData("01)08690504080008")]
        [InlineData("(0108690504080008")]
        [InlineData("(99)ABC")]
        [InlineData("(01)8690504080008")]
        [InlineData("(01)08690504080009")]
        [InlineData("(01)")]
        [InlineData("")]
        public void TryParse_WithInvalidInput_ReturnsFalseWithoutThrowing(string input)
        {
            Exception? exception = Record.Exception(
                () => Assert.False(Gs1ElementStringParser.TryParse(input, out var result)));

            Assert.Null(exception);
        }

        [Fact]
        public void Parse_WithDuplicateApplicationIdentifier_PreservesBothOccurrences()
        {
            const string input = "(01)08690504080008(01)08690504080008";

            IReadOnlyList<Gs1Element> result = Gs1ElementStringParser.Parse(input);

            Assert.Equal(2, result.Count);
            Assert.All(result, element => Assert.Equal("01", element.ApplicationIdentifier));
        }
    }
}
