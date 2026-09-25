using System;
using Game.Content;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Approved body art of an enemy definition for <see cref="EnemyFactory.Spawn"/>.
    /// Shared by ordinary enemies, bosses and Travelers so every spawner hands the same
    /// sprite/motion/contact to the runtime (bosses and Travelers were spawned without it and
    /// showed placeholder squares, DECISION-0057). Empty without a registry or visual reference:
    /// the runtime then uses its placeholder.</summary>
    public readonly struct EnemyBodyVisual
    {
        public Sprite Sprite { get; }
        public SpriteMotionProfile Motion { get; }
        public SpriteContactProfile Contact { get; }

        public EnemyBodyVisual(Sprite sprite, SpriteMotionProfile motion, SpriteContactProfile contact)
        {
            Sprite = sprite;
            Motion = motion;
            Contact = contact;
        }

        public static EnemyBodyVisual Resolve(EnemyDefinition definition, ContentRegistry registry)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (registry == null) return default;

            Sprite sprite = null;
            SpriteContactProfile contact = null;
            if (definition.Visual.TryResolve(registry, out var visual))
            {
                if (definition.MotionProfile.Id.IsValid) visual.RequireRole(SpriteRole.Body);
                sprite = visual.Sprite;
                contact = visual.Contact;
            }
            var motion = definition.MotionProfile.TryResolve(registry, out var profile) ? profile : null;
            return new EnemyBodyVisual(sprite, motion, contact);
        }
    }
}
