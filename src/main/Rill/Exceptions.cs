using System;

namespace Rill
{
    public static class Exceptions
    {
        public static EventOutOfOrderException EventOutOrOrder(Sequence expected, Sequence actual)
            => new(expected, actual);

        public static RillStoreIsCorruptException CorruptStoreMissingRillWhenWriting(RillReference reference)
            => new(reference, "No Rill was found and Commit Sequence does not represent first commit, hence state is corrupt");

        public static RillStoreConcurrencyException StoreConcurrency(RillReference reference, Sequence currentSequence, Sequence expectedSequence)
            => new(
                reference,
                currentSequence,
                expectedSequence,
                $"Rill with Reference:({reference}) has wrong sequence. Expected:({expectedSequence}) Actual:({currentSequence}).");
    }

    public class EventOutOfOrderException : Exception
    {
        public Sequence Expected { get; }
        public Sequence Actual { get; }

        public EventOutOfOrderException(Sequence expected, Sequence actual)
            : base($"Wrong Event sequence. Expected:({expected}) Actual:({actual}).")
        {
            Expected = expected;
            Actual = actual;
        }
    }

    public class RillStoreIsCorruptException : InvalidOperationException
    {
        public RillReference Reference { get; }

        public RillStoreIsCorruptException(
            RillReference reference,
            string message) : base(message)
        {
            Reference = reference;
        }
    }

    public class RillStoreConcurrencyException : InvalidOperationException
    {
        public RillReference Reference { get; }
        public Sequence CurrentSequence { get; }
        public Sequence ExpectedSequence { get; }

        public RillStoreConcurrencyException(
            RillReference reference,
            Sequence currentSequence,
            Sequence expectedSequence,
            string message) : base(message)
        {
            Reference = reference;
            CurrentSequence = currentSequence;
            ExpectedSequence = expectedSequence;
        }
    }
}
