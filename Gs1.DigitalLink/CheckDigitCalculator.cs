namespace Gs1.DigitalLink
{
    public static class CheckDigitCalculator
    {
        public static int Calculate(string digitsWithoutCheckDigit)
        {
            if (digitsWithoutCheckDigit.Length != 12)
            {
                throw new ArgumentException("Input must contain exactly 12 digits.");
            }

            int sum = 0;

            for (int i = 0; i < digitsWithoutCheckDigit.Length; i++)
            {
                char character = digitsWithoutCheckDigit[i];

                if (!char.IsDigit(character))
                {
                    throw new ArgumentException("Input must contain only digits.");
                }

                int digit = character - '0';
                int weight = i % 2 == 0 ? 1 : 3;

                sum += digit * weight;
            }

            return (10 - sum % 10) % 10;
        }
        public static bool IsValid(string value)
        {
            if (value.Length != 13)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsDigit(value[i]))
                {
                    return false;
                }
            }

            string firstTwelveDigits = value[..12];
            int expectedCheckDigit = Calculate(firstTwelveDigits);
            int actualCheckDigit = value[12] - '0';

            return expectedCheckDigit == actualCheckDigit;
        }
    }
}