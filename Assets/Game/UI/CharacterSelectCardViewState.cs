using Game.Content;
using UnityEngine;
namespace Game.UI
{
    public sealed class CharacterSelectCardViewState
    {
        public ContentId Id { get; }
        public ContentCardViewState Card { get; }
        public Sprite Crop { get; }
        public string Role { get; }
        public string Skill { get; }
        public string Boost { get; }
        public string Highlights { get; }
        public string Permanent { get; }
        public string LockReason { get; }
        public Sprite SkillIcon { get; }
        public System.Collections.Generic.IReadOnlyList<PermanentBonusRow> PermanentRows { get; }
        public CharacterSelectCardViewState(ContentId id, ContentCardViewState card, Sprite crop,
            string role = "", string skill = "", string boost = "", string highlights = "", string permanent = "", string lockReason = null, Sprite skillIcon = null,
            System.Collections.Generic.IReadOnlyList<PermanentBonusRow> permanentRows = null)
        {
            Id = id; Card = card; Crop = crop;
            Role = role; Skill = skill; Boost = boost; Highlights = highlights; Permanent = permanent; LockReason = lockReason;
            SkillIcon = skillIcon;
            PermanentRows = permanentRows ?? System.Array.Empty<PermanentBonusRow>();
        }
    }
}
