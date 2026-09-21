using System;
using Game.Character;
using Game.Combat;
using Game.Progression;
using Game.Run;
namespace Game.Pickup
{
    public sealed class PlayerPickupRewardTarget : IPickupRewardTarget
    {
        private readonly PlayerCharacterRuntime _player;
        private readonly RunModel _run;
        private readonly LevelUpDraftRuntime _draft;
        private readonly Action<SetRewardEvent> _rewarded;
        public PlayerPickupRewardTarget(PlayerCharacterRuntime player, RunModel run, LevelUpDraftRuntime draft, Action<SetRewardEvent> rewarded)
        { _player = player; _run = run; _draft = draft; _rewarded = rewarded; }
        public bool CanCollect(PickupIdentity identity) => identity.RunId == _run.RunId && _run.State == RunState.Running &&
            _player != null && _player.Health != null && !_player.Health.IsDead;
        public PickupRewardResult TryApply(PickupDefinition definition, PickupIdentity identity)
        {
            if (!CanCollect(identity)) return default;
            if (definition.Kind == PickupRewardKind.Book)
                return new PickupRewardResult(_draft.RequestBook(identity.DropId, identity.RunId, definition.Id));
            var source = new CombatSource(new CombatIdentity(identity.DropId, identity.RunId, definition.Id, CombatEntityCategory.Unknown),
                definition.Id, CombatSourceOrigin.Pickup);
            var result = _player.Heal(definition.Healing, source);
            if (CanCollect(identity)) _rewarded?.Invoke(new SetRewardEvent(source));
            return new PickupRewardResult(true, result.Health);
        }
    }
}
