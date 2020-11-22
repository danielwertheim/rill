using System;

namespace Rill.Internals
{
    internal static class Swallow
    {
        internal static void Everything(params Action[] actions)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < actions.Length; i++)
                try
                {
                    actions[i]();
                }
                catch
                {
                    // ignored
                }
        }
    }
}
