using Rill.Stores.InMemory;

namespace IntegrationTests.Stores
{
    public class InMemoryRillStoreTests : RillStoreTests<InMemoryRillStore>
    {
        public InMemoryRillStoreTests() : base(() => new InMemoryRillStore())
        {
        }
    }
}
