using Gs1.DigitalLink;
namespace Gs1.DigitalLink.Tests
{
    public class ApplicationIdentifierTests
    {
        [Theory]
        [InlineData("01", "08690504080008")]
        [InlineData("00", "869012345123456784")]
        [InlineData("10", "LOT123")]
        [InlineData("17", "261231")]
        [InlineData("414", "8690123456789")]
        [InlineData("3103", "000189")]
        [InlineData("240", "ABC123")]
        public void IsValid_WithMatchingApplicationIdentifierAndValue_ReturnsTrue(
            string applicationIdentifier,
            string value)
        {
            Assert.True(ApplicationIdentifierValidator.IsValid(applicationIdentifier, value));
        }
        [Theory]
        [InlineData("01", "8690504080008")]
        [InlineData("17", "26123")]
        [InlineData("17", "2612A1")]
        [InlineData("10", "")]
        [InlineData("01", "08690504080009")]
        [InlineData("414", "8690123456780")]
        [InlineData("00", "869012345123456785")]
        public void IsValid_WithNonMatchingValue_ReturnsFalse(
            string applicationIdentifier,
            string value)
        {
            Assert.False(ApplicationIdentifierValidator.IsValid(applicationIdentifier, value));
        }
        [Fact]
        public void IsValid_WithUnknownApplicationIdentifier_ReturnsFalse()
        {
            Assert.False(ApplicationIdentifierValidator.IsValid("99", "any-value"));
        }
        [Fact]
        public void Catalog_WithKnownApplicationIdentifier_ReturnsItsDefinition()
        {
            bool found = ApplicationIdentifierCatalog.TryGet("01", out var definition);
            Assert.True(found);
            Assert.NotNull(definition);
            Assert.Equal("01", definition.Code);
            Assert.Equal("GTIN", definition.Title);
            Assert.Equal("N14", definition.Format);
            Assert.True(definition.IsFixedLength);
            Assert.True(definition.HasCheckDigit);
            Assert.True(definition.IsNumeric);
            Assert.Equal(14, definition.MinLength);
            Assert.Equal(14, definition.MaxLength);
        }
        [Fact]
        public void Catalog_WithUnknownApplicationIdentifier_ReturnsFalse()
        {
            Assert.False(ApplicationIdentifierCatalog.TryGet("99", out var definition));
            Assert.Null(definition);
        }
    }
}