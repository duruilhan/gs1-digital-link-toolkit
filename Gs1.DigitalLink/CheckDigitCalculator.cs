namespace Gs1.DigitalLink
{
    public static class CheckDigitCalculator
    {
        private static readonly HashSet<int> ValidLengths = [8, 12, 13, 14, 18];
        private static readonly IReadOnlyDictionary<int, IReadOnlyList<Gs1KeyType>> KeyTypesByLength =
            new Dictionary<int, IReadOnlyList<Gs1KeyType>>
            {
                [8] = [Gs1KeyType.Gtin8],
                [12] = [Gs1KeyType.Gtin12],
                [13] = [Gs1KeyType.Gtin13, Gs1KeyType.Gln],
                [14] = [Gs1KeyType.Gtin14],
                [18] = [Gs1KeyType.Sscc]
            };
        public static int Calculate(string digitsWithoutCheckDigit)
        {
            ArgumentNullException.ThrowIfNull(digitsWithoutCheckDigit);

            if (!ValidLengths.Contains(digitsWithoutCheckDigit.Length + 1))
            {
                throw new ArgumentException("Input has an unsupported length.");
            }
            if (!digitsWithoutCheckDigit.All(char.IsDigit))
            {
                throw new ArgumentException("Input must contain only digits.");
            }
            int sum = 0;
            int weight = 3;
            for (int i = digitsWithoutCheckDigit.Length - 1; i >= 0; i--)
            {
                int digit = digitsWithoutCheckDigit[i] - '0';
                sum += digit * weight;
                weight = weight == 3 ? 1 : 3;
            }
            return (10 - sum % 10) % 10;
        }
        public static bool IsValid(string? value)
        {
            if (value is null || !ValidLengths.Contains(value.Length) || !value.All(char.IsDigit))
            {
                return false;
            }
            string digitsWithoutCheckDigit = value[..^1];
            int expectedCheckDigit = Calculate(digitsWithoutCheckDigit);
            int actualCheckDigit = value[^1] - '0';
            return expectedCheckDigit == actualCheckDigit;
        }
        public static IReadOnlyList<Gs1KeyType> GetPossibleKeyTypes(string? value)
        {
            if (!IsValid(value))
            {
                return [];
            }
            return KeyTypesByLength[value!.Length];
        }
    }
}