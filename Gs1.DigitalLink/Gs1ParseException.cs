namespace Gs1.DigitalLink
{
    public sealed class Gs1ParseException : FormatException
    {
        public Gs1ParseException(string message, int position)
            : base($"{message} (position {position}).")
        {
            Position = position;
        }
        public int Position { get; }
    }
}
