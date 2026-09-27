using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// One started boss special: exactly one of zone, beam or summon, attributed to <see cref="SourceId"/>
    /// (the sequence step carrier, or the boss itself for a dash-end zone).
    /// </summary>
    public readonly struct BossSpecialRequest
    {
        public BossSpecialKind Kind { get; }
        public BossZoneProfile Zone { get; }
        public BossBeamProfile Beam { get; }
        public BossSummonProfile Summon { get; }
        public ContentId SourceId { get; }

        private BossSpecialRequest(BossSpecialKind kind, BossZoneProfile zone, BossBeamProfile beam, BossSummonProfile summon,
            ContentId sourceId)
        {
            if (!sourceId.IsValid) throw new ArgumentException("A boss special needs its source id.", nameof(sourceId));
            Kind = kind;
            Zone = zone;
            Beam = beam;
            Summon = summon;
            SourceId = sourceId;
        }

        public static BossSpecialRequest ForZone(BossZoneProfile zone, ContentId source) =>
            new BossSpecialRequest(BossSpecialKind.Zone, zone ?? throw new ArgumentNullException(nameof(zone)), null, null, source);

        public static BossSpecialRequest ForBeam(BossBeamProfile beam, ContentId source) =>
            new BossSpecialRequest(BossSpecialKind.Beam, null, beam ?? throw new ArgumentNullException(nameof(beam)), null, source);

        public static BossSpecialRequest ForSummon(BossSummonProfile summon, ContentId source) =>
            new BossSpecialRequest(BossSpecialKind.Summon, null, null, summon ?? throw new ArgumentNullException(nameof(summon)), source);
    }
}
