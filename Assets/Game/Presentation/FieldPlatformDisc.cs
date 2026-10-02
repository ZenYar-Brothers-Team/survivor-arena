using UnityEngine;

namespace Game.Presentation
{
    /// <summary>One round platform of a platform field: center, radius and whether a Book lies at its center.</summary>
    public readonly struct FieldPlatformDisc
    {
        public Vector2 Center { get; }
        public float Radius { get; }
        public bool HasBook { get; }

        public FieldPlatformDisc(Vector2 center, float radius, bool hasBook = false)
        {
            Center = center;
            Radius = radius;
            HasBook = hasBook;
        }

        public FieldPlatformDisc WithBook(bool hasBook) => new FieldPlatformDisc(Center, Radius, hasBook);
    }
}
