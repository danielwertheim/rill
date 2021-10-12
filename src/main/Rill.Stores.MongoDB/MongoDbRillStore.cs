using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Rill.Stores.MongoDB
{
    public class MongoDbRillStore : IRillStore
    {
        private readonly IMongoClient _client;
        private readonly MongoDbRillStoreOptions _options;
        private readonly IMongoDatabase _database;
        private readonly ConcurrentDictionary<string, IMongoCollection<RillDoc>> _rills = new();
        private readonly ConcurrentDictionary<string, IMongoCollection<RillCommitDoc>> _commits = new();

        static MongoDbRillStore()
        {
            RillDoc.ConfigureSchema();
            RillCommitDoc.ConfigureSchema();
        }

        public MongoDbRillStore(IMongoClient client, MongoDbRillStoreOptions options)
        {
            _client = client;
            _options = options;
            _database = client.GetDatabase(options.DbName);
        }

        private IMongoCollection<RillDoc> GetRillsCollection(RillReference reference)
            => _rills.GetOrAdd(reference.Name, n => _database.GetCollection<RillDoc>(_options.CollectionPerRill ? $"{n}-rills" : "rills"));

        private IMongoCollection<RillCommitDoc> GetCommitsCollection(RillReference reference)
            => _commits.GetOrAdd(reference.Name, n => _database.GetCollection<RillCommitDoc>(_options.CollectionPerRill ? $"{n}-commits" : "commits"));

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
