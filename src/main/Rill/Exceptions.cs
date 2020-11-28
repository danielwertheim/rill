using System;

namespace Rill
{
    public static class Exceptions
    {
        public static EventOutOfOrderException EventOutOrOrder(Sequence expected, Sequence actual)
            => new EventOutOfOrderException(expected, actual);
    }

    public class EventOutOfOrderException : Exception
    {
        public Sequence Expected { get; }
        public Sequence Actual { get; }

        public EventOutOfOrderException(Sequence expected, Sequence actual)
            : base($"Event with sequence '{expected}' was expected. Got '{actual}'.")
        {
            Expected = expected;
            Actual = actual;
        }
    }
}
