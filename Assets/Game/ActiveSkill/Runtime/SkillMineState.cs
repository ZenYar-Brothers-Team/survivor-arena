using System;
using System.Collections.Generic;
using Game.Content;
using Game.Combat;
using Game.Diagnostics;
using Game.Enemy;
using Game.Movement;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    internal sealed class SkillMineState : IDisposable
    {
        private readonly GameObjectPool<SpriteRenderer> _pool;

        public ScheduledSkillEffect Scheduled { get; }
        public MineEffect Effect { get; }
        public GameObject Marker { get; }
        public ContentId SourceId => Scheduled.Activation.SourceId;
        public Vector2 Position => Marker != null ? (Vector2)Marker.transform.position : Scheduled.Activation.Origin;
        public float Elapsed { get; set; }

        public SkillMineState(ScheduledSkillEffect scheduled, MineEffect effect, GameObject marker, GameObjectPool<SpriteRenderer> pool)
        {
            Scheduled = scheduled;
            Effect = effect;
            Marker = marker;
            _pool = pool;
        }

        public void Dispose()
        {
            if (Marker == null)
                return;
            _pool.Return(Marker.GetComponent<SpriteRenderer>());
        }
    }
}
