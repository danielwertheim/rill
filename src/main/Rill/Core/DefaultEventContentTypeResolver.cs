using System;

namespace Rill.Core
{
    public class DefaultEventContentTypeResolver : IEventContentTypeResolver
    {
        public Type Resolve(EventContentType type)
            => Type.GetType($"{type.Namespace}.{type.Name}, {type.AssemblyName}", false, true) ??
               throw new TypeLoadException($"Could not load type for event content type: '{type}'.");
    }
}
