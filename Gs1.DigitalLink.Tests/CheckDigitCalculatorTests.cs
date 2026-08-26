using Gs1.DigitalLink;
namespace Gs1.DigitalLink.Tests
{
    public class CheckDigitCalculatorTests
    {
        [Theory]
        [InlineData("96385074")]
        [InlineData("96385005")]
        [InlineData("036000291452")]
        [InlineData("8690504080008")]
        [InlineData("5449000000996")]
        [InlineData("08690504080008")]
        [InlineData("8690123456789")]
        [InlineData("869012345123456784")]
        public void IsValid_WithSupportedGs1Numbers_ReturnsTrue(string input)
        {
            bool result = CheckDigitCalculator.IsValid(input);
            Assert.True(result);
        }
        [Fact]
        public void Calculate_WithTwelveDigits_ReturnsExpectedCheckDigit()
        {
            string input = "869050408000";
            int result = CheckDigitCalculator.Calculate(input);
            Assert.Equal(8, result);
        }
        [Theory]
        [InlineData("96385075")]
        [InlineData("036000291453")]
        [InlineData("8690504080009")]
        [InlineData("08690504080009")]
        public void IsValid_WithIncorrectCheckDigit_ReturnsFalse(string input)
        {
            bool result = CheckDigitCalculator.IsValid(input);

            Assert.False(result);
        }
        [Theory]
        [InlineData("96385074", Gs1KeyType.Gtin8)]
        [InlineData("036000291452", Gs1KeyType.Gtin12)]
        [InlineData("08690504080008", Gs1KeyType.Gtin14)]
        [InlineData("869012345123456784", Gs1KeyType.Sscc)]
        public void GetPossibleKeyTypes_WithUnambiguousNumber_ReturnsItsType(
            string input,
            Gs1KeyType expectedType)
        {
            IReadOnlyList<Gs1KeyType> result =
                CheckDigitCalculator.GetPossibleKeyTypes(input);

            Assert.Equal([expectedType], result);
        }
        [Fact]
        public void GetPossibleKeyTypes_WithThirteenDigits_ReturnsGtin13AndGln()
        {
            string input = "8690504080008";
            IReadOnlyList<Gs1KeyType> result =
                CheckDigitCalculator.GetPossibleKeyTypes(input);
            Assert.Equal([Gs1KeyType.Gtin13, Gs1KeyType.Gln], result);
        }
        [Fact]
        public void IsValid_WithUnsupportedLength_ThrowsArgumentException()
        {
            string input = "12345";
            Assert.Throws<ArgumentException>(
                () => CheckDigitCalculator.IsValid(input));
        }
        [Fact]
        public void IsValid_WithNonDigitCharacter_ThrowsArgumentException()
        {
            string input = "869O504080008";
            Assert.Throws<ArgumentException>(
                () => CheckDigitCalculator.IsValid(input));
        }
        [Theory]
        [InlineData("1234")]
        [InlineData("869O50408000")]
        public void Calculate_WithMalformedInput_ThrowsArgumentException(string input)
        {
            Assert.Throws<ArgumentException>(
                () => CheckDigitCalculator.Calculate(input));
        }
        [Theory]
        [InlineData("8690504080008")]
        [InlineData("8695004080008")]
        public void IsValid_WithKnownTranspositionLimitation_ReturnsTrue(string input)
        {
            bool result = CheckDigitCalculator.IsValid(input);
            Assert.True(result);
        }
    }
}