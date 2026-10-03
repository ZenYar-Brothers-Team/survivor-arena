using UnityEngine;

namespace Game.Traps
{
    /// <summary>
    /// Reference list of the 3D trap model prefabs, kept in Resources so the prefabs (which live with the art sources) ship with
    /// the game. Holds references only; every number about a model lives in the trap data (DECISION-0156).
    /// </summary>
    [CreateAssetMenu(menuName = "Survivor Arena/Trap Prefab Library")]
    public sealed class TrapPrefabLibrary : ScriptableObject
    {
        public const string ResourcePath = "Content/Presentation/TrapPrefabLibrary";

        public TrapPrefabEntry[] entries;

        public static TrapPrefabLibrary Load() => Resources.Load<TrapPrefabLibrary>(ResourcePath);

        /// <summary>The prefab registered under <paramref name="key"/>, or null.</summary>
        public GameObject Find(string key)
        {
            if (entries == null) return null;
            foreach (var entry in entries)
                if (entry != null && entry.key == key) return entry.prefab;
            return null;
        }
    }
}
