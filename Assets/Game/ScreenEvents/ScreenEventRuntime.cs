using System;
using System.Collections.Generic;
using Game.Diagnostics;
using UnityEngine;

namespace Game.ScreenEvents
{
    /// <summary>
    /// Simulation of a field's screen events (DECISION-0157). Pure C#: the scene driver ticks it only while the run is Running (so pause
    /// advances nothing) and draws what it exposes. Rules:
    /// <list type="bullet">
    /// <item>One event at a time. The pause before the next one starts when the previous one has finished.</item>
    /// <item>Intensity is a wave (<see cref="ScreenEventsDefinition.IntensityAt"/>): the pause shrinks and heavier events become likely as
    /// it rises; rare events only start near a peak and never closer together than the minimum interval.</item>
    /// <item>An event is picked from its stage's pool, weighted by how close its own intensity is to the current one; the event that
    /// just ran is skipped when there is another choice.</item>
    /// <item>A hazard hurts the player at most once, by a percentage of maximum health. Nothing here ever damages anything else.</item>
    /// <item>A boss being alive stops new events from starting (their timer waits); an event already running finishes.</item>
    /// </list>
    /// </summary>
    public sealed class ScreenEventRuntime
    {
        // Hit tests run at least this often inside one tick so a slow frame cannot skip a short strike.
        private const float MaxSubstepSeconds = 1f / 30f;
        private const float PauseJitter = .15f;

        private readonly ScreenEventsDefinition _definition;
        private readonly IScreenEventPlayerTarget _player;
        private readonly System.Random _random;
        private readonly List<ScreenEventPoolEntry> _candidates = new List<ScreenEventPoolEntry>();
        private float _lastRareEnd = float.NegativeInfinity;

        public ScreenEventsDefinition Definition => _definition;
        /// <summary>Running seconds since the run started.</summary>
        public float Elapsed { get; private set; }
        /// <summary>Current wave intensity, 0…1.</summary>
        public float Intensity => _definition.IntensityAt(Elapsed);
        /// <summary>Seconds until the next event starts (it counts down only while nothing runs and nothing suspends it).</summary>
        public float SecondsUntilNext { get; private set; }
        public ScreenEventInstance Active { get; private set; }
        public ScreenEventDefinition LastEvent { get; private set; }
        public int PlayerHitCount { get; private set; }
        /// <summary>Event waiting for a delayed manual start (a development tool); null when none.</summary>
        public string QueuedEventId { get; private set; }
        public float QueuedInSeconds { get; private set; }
        /// <summary>Events that ran although no fair layout was found within the attempt budget (0 on healthy content).</summary>
        public int UnfairStartCount { get; private set; }

        public event Action<ScreenEventInstance> EventStarted;
        public event Action<ScreenEventInstance> EventFinished;
        public event Action<ScreenEventInstance, ScreenHazard> PlayerHit;
        /// <summary>One notification per hazard at the telegraph-to-strike boundary, including strikes crossed in a large tick.</summary>
        public event Action<ScreenEventInstance, ScreenHazard> HazardStrikeStarted;

        public ScreenEventRuntime(ScreenEventsDefinition definition, IScreenEventPlayerTarget player, int seed)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _random = new System.Random(unchecked(seed * 6007 + 1301));
            SecondsUntilNext = definition.FirstDelaySeconds;
        }

        /// <summary>Advances the schedule and the running event by <paramref name="deltaSeconds"/> of running time.</summary>
        /// <param name="view">The rectangle the camera shows now; new events are laid out on it.</param>
        /// <param name="suspended">True while a boss is alive (and the content says bosses suspend events).</param>
        public void Tick(float deltaSeconds, Rect view, bool suspended)
        {
            Game.Content.NumericValidation.ValidateNonNegative(deltaSeconds, nameof(deltaSeconds));
            using var guard = PerfGuard.Measure("ScreenEvents.Tick", 2f);
            if (deltaSeconds <= 0f) return;
            Elapsed += deltaSeconds;
            if (QueuedEventId != null) QueuedInSeconds = Math.Max(0f, QueuedInSeconds - deltaSeconds);
            if (Active != null)
            {
                Advance(deltaSeconds);
                return;
            }
            if (QueuedEventId != null)
            {
                // A queued manual start replaces the schedule until it has fired.
                if (QueuedInSeconds > 0f) return;
                var queued = _definition.Events[QueuedEventId];
                QueuedEventId = null;
                StartEvent(queued, view);
                return;
            }
            if (suspended || !_player.IsAlive) return;
            SecondsUntilNext -= deltaSeconds;
            if (SecondsUntilNext <= 0f) StartEvent(PickEvent(), view);
        }

        /// <summary>Starts a specific event now, ignoring the schedule, e.g. from a development tool; false while one is running.</summary>
        public bool TryStart(string eventId, Rect view)
        {
            if (Active != null || !_definition.Events.TryGetValue(eventId, out var definition)) return false;
            StartEvent(definition, view);
            return true;
        }

        /// <summary>
        /// Starts a specific event after <paramref name="delaySeconds"/> of running time, ignoring the schedule (development tools).
        /// False for an unknown id or while an event is running or already queued. Until it fires no scheduled event starts.
        /// </summary>
        public bool TryQueue(string eventId, float delaySeconds)
        {
            Game.Content.NumericValidation.ValidateNonNegative(delaySeconds, nameof(delaySeconds));
            if (Active != null || QueuedEventId != null || eventId == null || !_definition.Events.ContainsKey(eventId)) return false;
            QueuedEventId = eventId;
            QueuedInSeconds = delaySeconds;
            return true;
        }

        /// <summary>Drops the running and the queued event, e.g. when the run ends.</summary>
        public void Clear()
        {
            Active = null;
            QueuedEventId = null;
        }

        private void StartEvent(ScreenEventDefinition definition, Rect view)
        {
            // Laying out and proving a fair event scans a grid of cells over the screen: cheap, but worth watching.
            using var guard = PerfGuard.Measure("ScreenEvents.Start", 40f);
            var position = _player.Position;
            ScreenEventInstance instance = null;
            for (var attempt = 0; attempt < _definition.FairnessAttempts; attempt++)
            {
                instance = ScreenEventSpawner.Create(definition, view, position, _definition.PlayerHitRadius, _random);
                if (ScreenEventSurvival.CanSurvive(instance, view, position, _definition.PlayerHitRadius, _definition.FairnessSpeed,
                        _definition.FairnessCellSize)) break;
                if (attempt == _definition.FairnessAttempts - 1) UnfairStartCount++;
            }
            Active = instance;
            LastEvent = definition;
            EventStarted?.Invoke(Active);
        }

        private ScreenEventDefinition PickEvent()
        {
            var intensity = Intensity;
            var stage = _definition.StageAt(Elapsed);
            var rareAllowed = intensity >= _definition.RareMinIntensity && Elapsed - _lastRareEnd >= _definition.RareMinIntervalSeconds;
            _candidates.Clear();
            foreach (var entry in stage.Pool)
                if (!entry.Event.Rare || rareAllowed) _candidates.Add(entry);
            if (_candidates.Count > 1) _candidates.RemoveAll(entry => entry.Event == LastEvent);
            var total = 0f;
            var weights = new float[_candidates.Count];
            for (var i = 0; i < weights.Length; i++)
            {
                var distance = (_candidates[i].Event.Intensity - intensity) / _definition.IntensityTolerance;
                weights[i] = _candidates[i].Weight * Mathf.Exp(-distance * distance);
                total += weights[i];
            }
            if (total <= 0f) return _candidates[0].Event;
            var roll = (float)_random.NextDouble() * total;
            for (var i = 0; i < weights.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return _candidates[i].Event;
            }
            return _candidates[weights.Length - 1].Event;
        }

        private void Advance(float deltaSeconds)
        {
            var instance = Active;
            var time = instance.Elapsed;
            var end = time + deltaSeconds;
            while (time < end)
            {
                var next = Math.Min(end, time + MaxSubstepSeconds);
                foreach (var hazard in instance.Hazards)
                {
                    var strikeAt = hazard.StartDelay + hazard.TelegraphSeconds;
                    if (time < strikeAt && next >= strikeAt) HazardStrikeStarted?.Invoke(instance, hazard);
                }
                time = next;
                CheckHits(instance, time);
            }
            instance.Elapsed = end;
            if (!instance.IsFinished) return;
            Active = null;
            if (instance.Definition.Rare) _lastRareEnd = Elapsed;
            var stage = _definition.StageAt(Elapsed);
            var pause = Mathf.Lerp(stage.PauseMaxSeconds, stage.PauseMinSeconds, Intensity);
            SecondsUntilNext = pause * (1f + ((float)_random.NextDouble() * 2f - 1f) * PauseJitter);
            EventFinished?.Invoke(instance);
        }

        private void CheckHits(ScreenEventInstance instance, float time)
        {
            if (!_player.IsAlive) return;
            var position = _player.Position;
            foreach (var hazard in instance.Hazards)
            {
                if (hazard.Hit || hazard.PhaseAt(time) != ScreenHazardPhase.Strike) continue;
                if (!hazard.Covers(hazard.StrikeTimeAt(time), position, _definition.PlayerHitRadius)) continue;
                hazard.Hit = true;
                PlayerHitCount++;
                _player.HitFraction(hazard.DamageFraction, instance.Definition.Id);
                PlayerHit?.Invoke(instance, hazard);
            }
        }
    }
}
