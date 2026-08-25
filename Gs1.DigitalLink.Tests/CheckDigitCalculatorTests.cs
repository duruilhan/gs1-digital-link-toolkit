using Gs1.DigitalLink;
namespace Gs1.DigitalLink.Tests
{
    public class CheckDigitCalculatorTests
    {
        [Fact]
        public void Calculate_WithTwelveDigits_ReturnsExpectedCheckDigit()
        {
            string input = "869050408000";
            int result = CheckDigitCalculator.Calculate(input);
            Assert.Equal(8, result);
        }
        [Fact]
        public void IsValid_WithCorrectCheckDigit_ReturnsTrue()
        {
            string input = "8690504080008";
            bool result = CheckDigitCalculator.IsValid(input);
            Assert.True(result);
        }
        [Theory]
        [InlineData("8690504080009")]
        [InlineData("5449000000990")]
        public void IsValid_WithIncorrectCheckDigit_ReturnsFalse(string input)
        {
            bool result = CheckDigitCalculator.IsValid(input);
            Assert.False(result);
        }
        [Theory]
        [InlineData("8691234567890")]
        [InlineData("5449000000996")]
        [InlineData("4006381333931")]
        [InlineData("9780143007234")]
        [InlineData("5901234123457")]
        public void IsValid_WithValidNumbers_ReturnsTrue(string input)
        {
            bool result = CheckDigitCalculator.IsValid(input);
            Assert.True(result);
        }
        [Theory]
        [InlineData("123456789012")]
        [InlineData("12345678901234")]
        [InlineData("869050408000A")]
        public void IsValid_WithInvalidFormat_ReturnsFalse(string input)
        {
            bool result = CheckDigitCalculator.IsValid(input);
            Assert.False(result);
        }
    }
}