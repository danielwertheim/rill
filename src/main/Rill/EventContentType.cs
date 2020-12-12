using System;

namespace Rill
{
    // public sealed record EventContentType(string AssemblyName, string Namespace, string Name);

    public sealed record EventContentType(string AssemblyName, string Namespace, string Name)
    {
        public static EventContentType From(Type type) =>
            new (
                type.Assembly.GetName().Name ?? string.Empty,
                type.Namespace ?? string.Empty,
                type.Name);
    }
}
