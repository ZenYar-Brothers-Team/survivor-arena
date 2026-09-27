using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// A boss sequence step that starts a zone, beam or summon (DECISION-0066) instead of firing projectiles. The step
    /// starts its special at the beginning of its turn and hands over to the next step <see cref="CooldownSeconds"/>
    /// later (the same wind-up-start-to-start cadence as projectile steps).
    /// </summary>
    public sealed class BossSpecialAttack
    {
        public float CooldownSeconds { get; }
        public BossSpecialKind Kind { get; }
        public BossZoneProfile Zone { get; }
        public BossBeamProfile Beam { get; }
        public BossSummonProfile Summon { get; }

        public BossSpecialAttack(float cooldownSeconds, BossZoneProfile zone = null, BossBeamProfile beam = null,
            BossSummonProfile summon = null)
        {
            NumericValidation.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            var count = (zone != null ? 1 : 0) + (beam != null ? 1 : 0) + (summon != null ? 1 : 0);
            if (count != 1) throw new ArgumentException("A boss special is exactly one of zone, beam or summon.");
            CooldownSeconds = cooldownSeconds;
            Zone = zone;
            Beam = beam;
            Summon = summon;
            Kind = zone != null ? BossSpecialKind.Zone : beam != null ? BossSpecialKind.Beam : BossSpecialKind.Summon;
        }

        public BossSpecialRequest ToRequest(ContentId source) => Kind switch
        {
            BossSpecialKind.Zone => BossSpecialRequest.ForZone(Zone, source),
            BossSpecialKind.Beam => BossSpecialRequest.ForBeam(Beam, source),
            _ => BossSpecialRequest.ForSummon(Summon, source)
        };
    }
}
