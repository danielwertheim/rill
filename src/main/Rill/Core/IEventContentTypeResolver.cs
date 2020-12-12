using System;

namespace Rill.Core
{
    public interface IEventContentTypeResolver
    {
        Type Resolve(EventContentType type);
    }
}
