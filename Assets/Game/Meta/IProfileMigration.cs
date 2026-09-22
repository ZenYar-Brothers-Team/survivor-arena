namespace Game.Meta
{
    /// <summary>Explicit one-version step; absence of a step blocks unknown old schemas.</summary>
    public interface IProfileMigration
    {
        int FromVersion { get; }
        string Migrate(string json);
    }
}
