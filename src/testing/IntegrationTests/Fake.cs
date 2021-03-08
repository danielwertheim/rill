using System;
using System.Linq;
using Rill;

namespace IntegrationTests
{
    internal static class Fake
    {
        internal static class Events
        {
            internal static Event Single(EventId? id = null, int sequenceSeed = 0)
                => Event.New(Strings.Random(), id ?? EventId.New(), sequenceSeed == 0 ? Sequence.First : Sequence.First.Add(sequenceSeed));

            internal static Event[] Many(int sequenceSeed = 0, int n = 3)
                => Enumerable.Range(sequenceSeed, n).Select(i => Single(sequenceSeed: i)).ToArray();
        }

        internal static class Strings
        {
            internal static string Random() => Guid.NewGuid().ToString("N");
        }
    }
}
