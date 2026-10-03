using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Spreads roundup participants evenly over rings around the player (DECISION-0155). Pure geometry with
    /// reusable buffers: the closest participants take the inner ring, a full ring spills into outer rings, and inside
    /// one ring the slot rotation and cyclic shift minimize how far everyone has to travel round the circle.</summary>
    public sealed class RingFormationPlanner
    {
        private float[] _keys = new float[64];
        private int[] _order = new int[64];
        private float[] _angles = new float[64];
        private int[] _members = new int[64];

        /// <summary>Fills <paramref name="offsets"/> with each participant's slot as an offset from the player and
        /// <paramref name="assigned"/> with whether it got one (beyond <paramref name="maxParticipants"/> it does not).
        /// Returns the number of assigned participants.</summary>
        public int Plan(Vector2 player, IReadOnlyList<Vector2> positions, float radius, float spacing, int maxParticipants,
            List<Vector2> offsets, List<bool> assigned)
        {
            if (positions == null || offsets == null || assigned == null) throw new ArgumentNullException();
            if (radius <= 0f || spacing <= 0f || maxParticipants <= 0) throw new ArgumentOutOfRangeException();
            var count = positions.Count;
            offsets.Clear();
            assigned.Clear();
            for (var i = 0; i < count; i++) { offsets.Add(Vector2.zero); assigned.Add(false); }
            if (count == 0) return 0;
            Ensure(count);
            for (var i = 0; i < count; i++)
            {
                _order[i] = i;
                _keys[i] = (positions[i] - player).sqrMagnitude;
            }
            Array.Sort(_keys, _order, 0, count);

            var take = Mathf.Min(count, maxParticipants);
            var cursor = 0;
            for (var ring = 0; cursor < take; ring++)
            {
                var ringRadius = radius + ring * spacing;
                var capacity = Mathf.Max(1, Mathf.FloorToInt(2f * Mathf.PI * ringRadius / spacing));
                var size = Mathf.Min(capacity, take - cursor);
                for (var m = 0; m < size; m++) _members[m] = _order[cursor + m];
                PlaceRing(player, positions, size, ringRadius, offsets, assigned);
                cursor += size;
            }
            return take;
        }

        private void PlaceRing(Vector2 player, IReadOnlyList<Vector2> positions, int size, float ringRadius,
            List<Vector2> offsets, List<bool> assigned)
        {
            for (var m = 0; m < size; m++)
            {
                var d = positions[_members[m]] - player;
                _angles[m] = Mathf.Atan2(d.y, d.x);
            }
            // Sort the ring's members by angle (members array is permuted alongside).
            Array.Sort(_angles, _members, 0, size);
            var step = 2f * Mathf.PI / size;

            // Try every cyclic shift of slot ownership; keep the one whose angular errors agree best.
            var bestShift = 0;
            var bestMagnitude = -1f;
            var bestRotation = 0f;
            for (var shift = 0; shift < size; shift++)
            {
                float sumSin = 0f, sumCos = 0f;
                for (var m = 0; m < size; m++)
                {
                    var error = _angles[m] - step * ((m + shift) % size);
                    sumSin += Mathf.Sin(error);
                    sumCos += Mathf.Cos(error);
                }
                var magnitude = sumSin * sumSin + sumCos * sumCos;
                if (magnitude > bestMagnitude)
                {
                    bestMagnitude = magnitude;
                    bestShift = shift;
                    bestRotation = Mathf.Atan2(sumSin, sumCos);
                }
            }
            for (var m = 0; m < size; m++)
            {
                var slot = bestRotation + step * ((m + bestShift) % size);
                var index = _members[m];
                offsets[index] = new Vector2(Mathf.Cos(slot), Mathf.Sin(slot)) * ringRadius;
                assigned[index] = true;
            }
        }

        private void Ensure(int count)
        {
            if (_keys.Length >= count) return;
            var length = Mathf.NextPowerOfTwo(count);
            _keys = new float[length];
            _order = new int[length];
            _angles = new float[length];
            _members = new int[length];
        }
    }
}
