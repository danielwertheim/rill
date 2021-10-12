using System;

namespace Rill.Stores.MongoDB
{
    public record MongoDbRillStoreOptions
    {
        public string DbName { get; }
        public bool CollectionPerRill { get; }

        public MongoDbRillStoreOptions(string dbName, bool collectionPerRill = true)
        {
            DbName = !string.IsNullOrWhiteSpace(dbName)
                ? dbName
                : throw new ArgumentException("A valid db-name must be provided.", nameof(dbName));
            CollectionPerRill = collectionPerRill;
        }
    }
}
