using System;

namespace Rill.Stores.EfCore.Serialization
{
    public interface IEventContentSerializer
    {
        string Serialize<T>(T content) where T : class;
        object Deserialize(string content, Type type);
    }
}
