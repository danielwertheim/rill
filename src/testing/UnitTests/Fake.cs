using System;

namespace UnitTests
{
    internal static class Fake
    {
        internal static class Strings
        {
            internal static string Random() => Guid.NewGuid().ToString("N");
        }
    }
}
