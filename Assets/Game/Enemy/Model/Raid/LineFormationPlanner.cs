using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Row-based roundup formations (DECISION-0155): wall and pincer. Slots are offsets from the player
    /// in world axes; <c>forward</c> is the unit direction the formation faces. The closest participants fill the rows
    /// nearest the player, and inside a row members keep their sideways order so paths do not cross.</summary>
    public sealed class LineFormationPlanner
    {
        private float[] _keys = new float[64];
        private int[] _order = new int[64];
        private float[] _lateral = new float[64];
        private int[] _members = new int[64];
        private readonly List<int> _left = new List<int>();
        private readonly List<int> _right = new List<int>();

        /// <summary>A straight wall ahead of the player: rows of full width, extra rows stacked behind it.</summary>
        public int PlanWall(Vector2 player, IReadOnlyList<Vector2> positions, Vector2 forward, float distance, float width,
            float spacing, int maxParticipants, List<Vector2> offsets, List<bool> assigned)
        {
            var take = Begin(player, positions, forward * distance, spacing, maxParticipants, offsets, assigned);
            var perp = Perp(forward);
            var capacity = Mathf.Max(1, Mathf.FloorToInt(width / spacing) + 1);
            var cursor = 0;
            for (var row = 0; cursor < take; row++)
            {
                var size = Mathf.Min(capacity, take - cursor);
                var axis = distance + row * spacing;
                SortRow(player, positions, perp, cursor, size);
                for (var m = 0; m < size; m++)
                {
                    var lateral = (2f * ((m + .5f) / size) - 1f) * width * .5f;
                    Put(offsets, assigned, _members[m], forward * axis + perp * lateral);
                }
                cursor += size;
            }
            return take;
        }

        /// <summary>Two columns parallel to <paramref name="forward"/>, one on each side of the player, <paramref name="gap"/> apart.</summary>
        public int PlanPincer(Vector2 player, IReadOnlyList<Vector2> positions, Vector2 forward, float length, float gap,
            float spacing, int maxParticipants, List<Vector2> offsets, List<bool> assigned)
        {
            var count = positions.Count;
            Reset(count, offsets, assigned);
            var perp = Perp(forward);
            _left.Clear();
            _right.Clear();
            Ensure(count);
            for (var i = 0; i < count; i++) _keys[i] = (positions[i] - player).sqrMagnitude;
            for (var i = 0; i < count; i++) _order[i] = i;
            Array.Sort(_keys, _order, 0, count);
            var take = Mathf.Min(count, maxParticipants);
            for (var i = 0; i < take; i++)
            {
                var index = _order[i];
                (Vector2.Dot(positions[index] - player, perp) >= 0f ? _left : _right).Add(index);
            }
            PlaceColumns(player, positions, _left, forward, perp, +1f, length, gap, spacing, offsets, assigned);
            PlaceColumns(player, positions, _right, forward, perp, -1f, length, gap, spacing, offsets, assigned);
            return take;
        }

        private void PlaceColumns(Vector2 player, IReadOnlyList<Vector2> positions, List<int> side, Vector2 forward, Vector2 perp,
            float sign, float length, float gap, float spacing, List<Vector2> offsets, List<bool> assigned)
        {
            var capacity = Mathf.Max(1, Mathf.FloorToInt(length / spacing) + 1);
            var cursor = 0;
            for (var column = 0; cursor < side.Count; column++)
            {
                var size = Mathf.Min(capacity, side.Count - cursor);
                var lateral = sign * (gap * .5f + column * spacing);
                for (var m = 0; m < size; m++)
                {
                    _members[m] = side[cursor + m];
                    _lateral[m] = Vector2.Dot(positions[_members[m]] - player, forward);
                }
                Array.Sort(_lateral, _members, 0, size);
                for (var m = 0; m < size; m++)
                {
                    var along = (2f * ((m + .5f) / size) - 1f) * length * .5f;
                    Put(offsets, assigned, _members[m], forward * along + perp * lateral);
                }
                cursor += size;
            }
        }

        // Orders every participant by distance to the formation anchor (nearest first) and clears the outputs.
        private int Begin(Vector2 player, IReadOnlyList<Vector2> positions, Vector2 anchorOffset, float spacing,
            int maxParticipants, List<Vector2> offsets, List<bool> assigned)
        {
            if (spacing <= 0f || maxParticipants <= 0) throw new ArgumentOutOfRangeException();
            var count = positions.Count;
            Reset(count, offsets, assigned);
            if (count == 0) return 0;
            Ensure(count);
            var anchor = player + anchorOffset;
            for (var i = 0; i < count; i++)
            {
                _order[i] = i;
                _keys[i] = (positions[i] - anchor).sqrMagnitude;
            }
            Array.Sort(_keys, _order, 0, count);
            return Mathf.Min(count, maxParticipants);
        }

        // Loads the next `size` participants of the distance order into _members, sorted by sideways coordinate.
        // The distance order is consumed in rows, so the caller passes the running cursor.
        private void SortRow(Vector2 player, IReadOnlyList<Vector2> positions, Vector2 perp, int cursor, int size)
        {
            for (var m = 0; m < size; m++)
            {
                _members[m] = _order[cursor + m];
                _lateral[m] = Vector2.Dot(positions[_members[m]] - player, perp);
            }
            Array.Sort(_lateral, _members, 0, size);
        }

        private static Vector2 Perp(Vector2 forward) => new Vector2(-forward.y, forward.x);

        private static void Put(List<Vector2> offsets, List<bool> assigned, int index, Vector2 offset)
        {
            offsets[index] = offset;
            assigned[index] = true;
        }

        private static void Reset(int count, List<Vector2> offsets, List<bool> assigned)
        {
            offsets.Clear();
            assigned.Clear();
            for (var i = 0; i < count; i++) { offsets.Add(Vector2.zero); assigned.Add(false); }
        }

        private void Ensure(int count)
        {
            if (_keys.Length >= count) return;
            var length = Mathf.NextPowerOfTwo(count);
            _keys = new float[length];
            _order = new int[length];
            _lateral = new float[length];
            _members = new int[length];
        }
    }
}
