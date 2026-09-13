namespace Game.Content
{
    public readonly struct ContentRef<T> where T : class, IContentDefinition
    {
        public ContentId Id { get; }

        public ContentRef(ContentId id)
        {
            Id = id;
        }

        public T Resolve(ContentRegistry registry) => registry.Get<T>(Id);

        public bool TryResolve(ContentRegistry registry, out T definition) =>
            registry.TryGet(Id, out definition);

        public ContentReference ToReference() => new ContentReference(Id, typeof(T));
    }
}
