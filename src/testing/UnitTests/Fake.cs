using System;
using System.Linq;
using Bogus;
using Rill;

namespace UnitTests
{
    internal class Dummy
    {
        internal string Value { get; }

        public Dummy(string value)
        {
            Value = value;
        }
    }

    internal static class Fake
    {
        private static readonly Faker LocalFaker = new("en");

        internal static class Events
        {
            internal static Event Single(EventId? id = null, int sequenceSeed = 0)
                => Event.New(Strings.RandomAlphaNumericUpperAndLowerCase(), id ?? EventId.New(), sequenceSeed == 0 ? Sequence.First : Sequence.First.Add(sequenceSeed));

            internal static Event[] Many(int sequenceSeed = 0, int n = 3)
                => Enumerable.Range(sequenceSeed, n).Select(i => Single(sequenceSeed: i)).ToArray();
        }

        internal static class Strings
        {
            private static readonly string AlphaNumericUpperAndLowerCase = $"{Chars.AlphaNumericLowerCase}{Chars.AlphaNumericUpperCase}";
            private static readonly string UpperAndLowerCase = $"{Chars.LowerCase}{Chars.UpperCase}";

            internal static string RandomAlphaNumericUpperCase(int length = 32) => LocalFaker.Random.String2(length, Chars.AlphaNumericUpperCase);
            internal static string RandomAlphaNumericLowerCase(int length = 32) => LocalFaker.Random.String2(length, Chars.AlphaNumericLowerCase);
            internal static string RandomAlphaNumericUpperAndLowerCase(int length = 32) => LocalFaker.Random.String2(length, AlphaNumericUpperAndLowerCase);


            internal static string RandomLettersUpperCase(int length = 32) => LocalFaker.Random.String2(length, Chars.UpperCase);
            internal static string RandomLettersLowerCase(int length = 32) => LocalFaker.Random.String2(length, Chars.LowerCase);

            internal static string RandomLettersUpperAndLowerCase(int length = 32) => LocalFaker.Random.String2(length, Chars.LowerCase);

        }
    }
}
