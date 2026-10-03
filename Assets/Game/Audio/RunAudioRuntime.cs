using System;
using System.Collections.Generic;
using Game.ActiveSkill;
using Game.Combat;
using Game.Enemy;
using Game.Pickup;
using Game.Progression;
using Game.Run;
using Game.Settings;
using Game.Traveler;
using Game.Zones;
using Game.ScreenEvents;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>One run's bounded, event-driven sound layer. Projectiles never own audio sources.</summary>
    public sealed class RunAudioRuntime : IDisposable
    {
        private const int RoutineVoices = 8;
        private const int ImportantVoices = 2;
        private const int UiVoices = 2;
        private readonly GameObject _owner;
        private readonly AudioSource[] _voices = new AudioSource[RoutineVoices + ImportantVoices + UiVoices];
        private readonly int[] _priorities = new int[RoutineVoices + ImportantVoices + UiVoices];
        private readonly double[] _started = new double[RoutineVoices + ImportantVoices + UiVoices];
        private readonly float[] _gains = new float[RoutineVoices + ImportantVoices + UiVoices];
        private readonly Dictionary<string, double> _lastPlayed = new Dictionary<string, double>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _variants = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly ProductionAudioCatalog _catalog;
        private readonly ISettingsService _settings;
        private readonly AudioRoutingRuntime _music;
        private readonly RunModel _run;
        private readonly Health _health;
        private readonly PlayerExperienceRuntime _xp;
        private readonly LevelUpDraftRuntime _draft;
        private readonly PlayerActiveSkillSetRuntime _skills;
        private readonly WorldPickupRuntime _pickups;
        private readonly ContinuousFixtureEnemySpawner _enemies;
        private readonly BossEncounterRuntime _bosses;
        private readonly TravelerEncounterRuntime _travelers;
        private readonly ZoneRuntime _zones;
        private readonly ScreenEventRuntime _screenEvents;
        private readonly Func<Rect> _visibleArea;
        private readonly AudioSource _ambience;
        private readonly float _ambienceGain;
        private int _livingBosses;
        private double _lastRoutine = double.NegativeInfinity;

        public RunAudioRuntime(Transform parent, ProductionAudioCatalog catalog, ISettingsService settings,
            AudioRoutingRuntime music, RunModel run, Health health, PlayerExperienceRuntime xp,
            LevelUpDraftRuntime draft, PlayerActiveSkillSetRuntime skills, WorldPickupRuntime pickups,
            ContinuousFixtureEnemySpawner enemies, BossEncounterRuntime bosses, string fieldId,
            TravelerEncounterRuntime travelers = null, ZoneRuntime zones = null, Func<Rect> visibleArea = null,
            ScreenEventRuntime screenEvents = null)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _music = music ?? throw new ArgumentNullException(nameof(music));
            _run = run; _health = health; _xp = xp; _draft = draft; _skills = skills;
            _pickups = pickups; _enemies = enemies; _bosses = bosses;
            _travelers = travelers; _zones = zones; _visibleArea = visibleArea;
            _screenEvents = screenEvents;
            _owner = new GameObject("Run audio"); _owner.transform.SetParent(parent, false);
            for (var i = 0; i < _voices.Length; i++) _voices[i] = Source();
            _ambience = Source(); _ambience.loop = true;
            var hasAmbience = catalog.TryGetAmbience(fieldId, out var ambienceClip, out _ambienceGain);
            _ambience.clip = ambienceClip;
            _settings.Changed += Refresh;
            _run.StateChanged += State;
            _run.Completed += Completed;
            _health.Damaged += Hurt;
            _xp.ExperienceResolved += Experience;
            _xp.LevelUp += Level;
            _draft.DraftOpened += DraftOpened;
            _draft.SelectionApplied += Selected;
            _skills.Activated += Activated;
            _pickups.Resolved += Pickup;
            _enemies.LifeEvent += Enemy;
            _enemies.CombatResolved += EnemyHit;
            _bosses.LifeEvent += Boss;
            _bosses.CombatResolved += BossHit;
            _bosses.PhaseChanged += BossPhase;
            _enemies.ActionStarted += EnemyAction;
            _bosses.ActionStarted += EnemyAction;
            if (_travelers != null) _travelers.ActionStarted += EnemyAction;
            if (_zones != null) _zones.Triggered += ZoneTriggered;
            if (_screenEvents != null) _screenEvents.HazardStrikeStarted += ScreenStrike;
            Refresh();
            _music.PlayMusic(catalog.BattleMusic);
            if (hasAmbience) _ambience.Play();
            State(run.State);
        }

        private AudioSource Source()
        {
            var source = _owner.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0; source.ignoreListenerPause = true;
            source.mute = Application.isBatchMode;
            return source;
        }

        private void Refresh()
        {
            for (var i = 0; i < _voices.Length; i++)
                _voices[i].volume = _settings.Current.Gain(false, _gains[i]);
            _ambience.volume = _settings.Current.Gain(true, _ambienceGain);
        }

        public bool Play(string id)
        {
            if (!_catalog.TryGet(id, out var cue, out var clips)) return false;
            if (!cue.Ui.Value && _run.State != RunState.Running) return false;
            var now = Time.realtimeSinceStartupAsDouble;
            if (_lastPlayed.TryGetValue(id, out var last) && now - last < cue.CooldownSeconds.Value) return false;
            if (!cue.Ui.Value && cue.Priority.Value == 0 &&
                now - _lastRoutine < _catalog.RoutineGlobalCooldownSeconds) return false;
            var first = cue.Ui.Value ? RoutineVoices + ImportantVoices :
                cue.Priority.Value == 0 ? 0 : RoutineVoices;
            var end = cue.Ui.Value ? _voices.Length :
                cue.Priority.Value == 0 ? RoutineVoices : RoutineVoices + ImportantVoices;
            var slot = -1;
            for (var i = first; i < end; i++)
                if (!_voices[i].isPlaying) { slot = i; break; }
            if (slot < 0 && cue.Priority.Value > 0)
            {
                for (var i = first; i < end; i++)
                    if (_priorities[i] <= cue.Priority.Value && (slot < 0 || _started[i] < _started[slot])) slot = i;
            }
            if (slot < 0) return false;
            var variant = _variants.TryGetValue(id, out var previous) ? previous : 0;
            _voices[slot].Stop();
            _voices[slot].clip = clips[variant % clips.Length];
            _gains[slot] = cue.Gain.Value;
            _voices[slot].volume = _settings.Current.Gain(false, _gains[slot]);
            _priorities[slot] = cue.Priority.Value;
            _started[slot] = now;
            _lastPlayed[id] = now;
            if (!cue.Ui.Value && cue.Priority.Value == 0) _lastRoutine = now;
            _variants[id] = variant + 1;
            _voices[slot].Play();
            return true;
        }

        private void State(RunState state)
        {
            if (state == RunState.Paused)
                for (var i = 0; i < RoutineVoices + ImportantVoices; i++) _voices[i].Pause();
            else if (state == RunState.Running)
                for (var i = 0; i < RoutineVoices + ImportantVoices; i++) _voices[i].UnPause();
            else
                for (var i = 0; i < RoutineVoices + ImportantVoices; i++) _voices[i].Stop();
        }

        private void Hurt(float amount) => Play("player.hurt");
        private void ScreenStrike(ScreenEventInstance instance, ScreenHazard hazard)
        {
            if (_catalog.TryGetScreenEventCue(instance.Definition.Id.ToString(), out var cue)) Play(cue);
        }
        private void Experience(ExperienceAwardEvent award)
        {
            if (award.Kind == ExperienceEventKind.Collected && award.AwardedAmount > 0) Play("xp.pickup");
        }
        private void Level(int level) => Play("level.up");
        private void DraftOpened(IReadOnlyList<DraftOption> options) => Play("draft.open");
        private void Selected(BuildSelectionResult result) => Play("draft.select");
        private void Activated(CombatSource source)
        {
            // Each sound family has its own cooldown, so a build with several skills is heard as several rhythms.
            if (source.ContentId.HasValue && _catalog.TryGetSkillCue(source.ContentId.Value.ToString(), out var cue)) Play(cue);
            else Play("skill.cast");
        }
        // World-positioned cues are heard only when they happen on screen; no camera means everything is audible.
        private bool OnScreen(Vector2 position)
        {
            var view = _visibleArea?.Invoke() ?? default;
            return view.width <= 0f || view.height <= 0f || view.Contains(position);
        }

        private void EnemyAction(EnemyActionKind kind, Vector2 position)
        {
            if (!OnScreen(position)) return;
            switch (kind)
            {
                case EnemyActionKind.AttackWindup: Play("enemy.windup"); break;
                case EnemyActionKind.Shot: Play("enemy.shot"); break;
                case EnemyActionKind.DashWindup: Play("enemy.dash.windup"); break;
                case EnemyActionKind.DashStart: Play("enemy.dash"); break;
                case EnemyActionKind.ZoneStart: Play("boss.zone"); break;
                case EnemyActionKind.BeamStart: Play("boss.beam"); break;
                case EnemyActionKind.SummonStart: Play("boss.summon"); break;
                case EnemyActionKind.TeleportWindup: Play("boss.teleport.windup"); break;
                case EnemyActionKind.TeleportSlam: Play("boss.slam"); break;
            }
        }
        private void ZoneTriggered(ZoneTrigger trigger)
        {
            if (trigger.Position.HasValue && !OnScreen(trigger.Position.Value)) return;
            switch (trigger.Kind)
            {
                case ZoneTriggerKind.Activated:
                    Play(trigger.Polarity == ZoneAltarPolarity.Positive ? "altar.on" :
                        trigger.Polarity.HasValue ? "altar.cursed" : "zone.activate");
                    break;
                case ZoneTriggerKind.ShrineReward: Play("shrine.reward"); break;
                case ZoneTriggerKind.StrikeImpact: Play("zone.strike"); break;
                case ZoneTriggerKind.BurstFired: Play("zone.burst"); break;
                case ZoneTriggerKind.PortalJump: Play("zone.portal"); break;
            }
        }
        private void Pickup(PickupEvent pickup)
        {
            if (pickup.State == PickupLifeState.Collected)
                Play(pickup.Kind == PickupRewardKind.Book ? "book.pickup" : "potion.pickup");
        }
        private void Enemy(EnemyLifeEvent enemy)
        {
            if (enemy.Kind == EnemyLifeEventKind.Died) Play("enemy.fall");
        }
        private void EnemyHit(CombatResult result)
        {
            if (result.Health.Actual > 0 && !result.Health.IsHealing) Play("enemy.hit");
        }
        private void BossHit(CombatResult result)
        {
            if (result.Health.Actual > 0 && !result.Health.IsHealing) Play("boss.hit");
        }
        private void Boss(EnemyLifeEvent boss)
        {
            if (boss.Kind == EnemyLifeEventKind.Spawned)
            {
                _livingBosses++;
                Play("boss.warning");
                _music.PlayMusic(_catalog.BossMusic);
            }
            else if (boss.Kind == EnemyLifeEventKind.Died || boss.Kind == EnemyLifeEventKind.Despawned)
            {
                _livingBosses = Math.Max(0, _livingBosses - 1);
                if (_livingBosses == 0 && _run.State == RunState.Running)
                    _music.PlayMusic(_catalog.BattleMusic);
            }
        }
        private void BossPhase(BossPhaseEvent phase) => Play("boss.warning");
        private void Completed(RunOutcome outcome)
        {
            _ambience.Stop();
            if (outcome.Reason == RunCompletionReason.Victory) _music.PlayMusic(_catalog.VictoryMusic, loop: false);
            else if (outcome.Reason == RunCompletionReason.Defeat) _music.PlayMusic(_catalog.DefeatMusic, loop: false);
            else _music.StopMusic();
        }

        public void Dispose()
        {
            _settings.Changed -= Refresh;
            _run.StateChanged -= State;
            _run.Completed -= Completed;
            _health.Damaged -= Hurt;
            _xp.ExperienceResolved -= Experience;
            _xp.LevelUp -= Level;
            _draft.DraftOpened -= DraftOpened;
            _draft.SelectionApplied -= Selected;
            _skills.Activated -= Activated;
            _pickups.Resolved -= Pickup;
            _enemies.LifeEvent -= Enemy;
            _enemies.CombatResolved -= EnemyHit;
            _bosses.LifeEvent -= Boss;
            _bosses.CombatResolved -= BossHit;
            _bosses.PhaseChanged -= BossPhase;
            _enemies.ActionStarted -= EnemyAction;
            _bosses.ActionStarted -= EnemyAction;
            if (_travelers != null) _travelers.ActionStarted -= EnemyAction;
            if (_zones != null) _zones.Triggered -= ZoneTriggered;
            if (_screenEvents != null) _screenEvents.HazardStrikeStarted -= ScreenStrike;
            UnityEngine.Object.Destroy(_owner);
        }
    }
}
