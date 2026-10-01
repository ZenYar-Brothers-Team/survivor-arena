using UnityEngine;
namespace Game.UI
{
    /// <summary>One personal meta upgrade of a character in the character window: icon, name and the current bonus (zero when not bought).</summary>
    public sealed class PermanentBonusRow
    {
        public Sprite Icon { get; }
        public string Name { get; }
        public string Value { get; }
        public PermanentBonusRow(Sprite icon, string name, string value) { Icon = icon; Name = name; Value = value; }
    }
}
