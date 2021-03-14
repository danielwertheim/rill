using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Rill.Stores.MongoDB
{
    public record MongoDbRillStoreOptions(string DbName);

    internal class RillDocument
    {
        internal string Reference { get; }
        internal string Name { get; }
        internal long LastSequence { get; private set; }
        internal DateTimeOffset CreatedAt { get; }
        internal DateTimeOffset LastChangedAt { get; private set; }

        private RillDocument(
            string reference,
            string name,
            Guid id,
            long lastSequence,
            DateTimeOffset createdAt,
            DateTimeOffset lastChangedAt)
        {
            Reference = reference;
            Name = name;
            LastSequence = lastSequence;
            CreatedAt = createdAt;
            LastChangedAt = lastChangedAt;
        }

        internal static void Configure()
        {
            BsonClassMap.RegisterClassMap<RillDocument>(schema =>
            {
                schema.AutoMap();
                // schema
                //     .MapIdMember(d => d.Id)
                //     .SetIdGenerator(NullIdChecker.Instance);
            });
        }
    }

    internal class RillCommitDocument
    {
    }

    public class MongoDbRillStore : IRillStore
    {
        private readonly IMongoClient _client;
        private readonly IMongoCollection<RillDocument> _rills;
        private readonly IMongoCollection<RillCommitDocument> _commits;

        public MongoDbRillStore(IMongoClient client, MongoDbRillStoreOptions options)
        {
            _client = client;

            var database = client.GetDatabase(options.DbName);
            _rills = database.GetCollection<RillDocument>("Rills");
            _commits = database.GetCollection<RillCommitDocument>("RillCommits");
        }

        public Task<RillDetails?> GetDetailsAsync(RillReference reference, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task AppendAsync(RillCommit commit, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(RillReference reference, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<RillCommit> ReadCommits(RillReference reference, SequenceRange? sequenceRange = default)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<RillCommit> ReadCommitsAsync(RillReference reference, SequenceRange? sequenceRange = default,
            CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
