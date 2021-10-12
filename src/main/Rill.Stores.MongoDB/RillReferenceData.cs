namespace Rill.Stores.MongoDB
{
    internal class RillReferenceData
    {
        public string Name { get; }
        public string Id { get; }

        private RillReferenceData(string name, string id)
        {
            Name = name;
            Id = id;
        }

        internal static RillReferenceData From(RillReference reference)
            => new(reference.Name, reference.Id);
    }
}
