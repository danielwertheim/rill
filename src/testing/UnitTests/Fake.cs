using System;
using System.Linq;
using Rill;

namespace UnitTests
{
    internal static class Fake
    {
        internal static class Events
        {
            internal static Event<string> Single(int sequenceSeed = 0)
                => Event<string>.Create(Fake.Strings.Random(), EventId.New(), sequenceSeed == 0 ? Sequence.First : Sequence.First.Add(sequenceSeed));

            internal static Event<string>[] Many(int sequenceSeed = 0, int n = 3)
                => Enumerable.Range(sequenceSeed, n).Select(Single).ToArray();
        }

        internal static class Strings
        {
            internal static string Random() => Guid.NewGuid().ToString("N");
        }
    }
}
