namespace Rill.Stores.MongoDB
{
    internal class RillSequenceRangeData
    {
        public long Lower { get; }
        public long Upper { get; }

        private RillSequenceRangeData(long lower, long upper)
        {
            Lower = lower;
            Upper = upper;
        }

        internal static RillSequenceRangeData From(SequenceRange sequenceRange)
            => new((long)sequenceRange.Lower, (long)sequenceRange.Upper);
    }
}
