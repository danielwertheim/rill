using System;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Rill.Stores.EfCore.Serialization
{
    public class JsonEventContentSerializer : IEventContentSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonEventContentSerializer(JsonSerializerOptions? options = default)
            => _options = options ?? new JsonSerializerOptions();

        public string Serialize<T>(T content) where T : class
            => JsonSerializer.Serialize(content, content.GetType(), _options);

        public object Deserialize(string content, Type type)
            => JsonSerializer.Deserialize(content, type, _options) ?? throw new SerializationException();
    }
}
