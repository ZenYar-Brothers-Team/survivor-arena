using System;
using UnityEngine;

namespace Game.Traps
{
    /// <summary>One 3D model prefab of the trap library, found by the key a start trap or model definition names.</summary>
    [Serializable]
    public sealed class TrapPrefabEntry
    {
        public string key;
        public GameObject prefab;
    }
}
