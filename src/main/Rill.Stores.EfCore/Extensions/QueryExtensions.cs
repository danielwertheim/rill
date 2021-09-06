using System.Linq;

namespace Rill.Stores.EfCore.Extensions
{
    internal static class QueryExtensions
    {
        private static IQueryable<RillCommitEntity> Matching(this IQueryable<RillCommitEntity> commits, RillReference reference) =>
            commits.Where(c => c.RillName == reference.Name && c.RillId == reference.Id);

        private static IQueryable<RillCommitEntity> Matching(this IQueryable<RillCommitEntity> commits, SequenceRange range)
        {
            var seekedLower = (long) range.Lower;
            var seekedUpper = (long) range.Upper;

            //a) the commits lower bound is between seeked range
            //b) the commits upper bound is between seeked range
            //c) the commits range spans over the seeked range

            return commits
                .Where(c => (c.SequenceLowerBound >= seekedLower && c.SequenceLowerBound <= seekedUpper) ||
                            (c.SequenceUpperBound >= seekedLower && c.SequenceUpperBound <= seekedUpper) ||
                            (c.SequenceLowerBound <= seekedLower && c.SequenceUpperBound >= seekedUpper));
        }

        internal static IQueryable<RillCommitEntity> Matching(this IQueryable<RillCommitEntity> commits, RillReference reference, SequenceRange range)
            => range == SequenceRange.Any
                ? commits.Matching(reference)
                : commits.Matching(reference).Matching(range);
    }
}
