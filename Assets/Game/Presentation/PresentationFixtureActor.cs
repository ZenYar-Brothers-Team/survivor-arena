using System;
using Game.Movement;
using UnityEngine;

namespace Game.Presentation
{
    // Synthetic source for diagnostics/tests only. Owning gameplay modules implement IPresentationSource.
    public sealed class PresentationFixtureActor : MonoBehaviour, IPresentationSource
    {
        public bool IsRunning { get; set; }
        public Vector2 Velocity { get; set; }
        public event Action<PresentationSignal> Signaled;
        public SpriteRenderer Renderer { get; private set; }
        public SpritePresentationAdapter Adapter { get; private set; }
        public SpriteRole Role { get; private set; }

        public void Initialize(PresentationFixtureDefinition definition, PresentationFeedbackProfile feedback,
            SpriteMotionProfile motion)
        {
            Shutdown();
            if (Renderer == null)
            {
                var visual = new GameObject("VisualRoot");
                visual.transform.SetParent(transform, false);
                Renderer = visual.AddComponent<SpriteRenderer>();
            }
            Renderer.transform.localScale = new Vector3(definition.Size.x, definition.Size.y, 1);
            Renderer.color = definition.Color;
            Renderer.sortingOrder = definition.Role == SpriteRole.Shadow ? -1 : 1;
            Role = definition.Role;
            Adapter = new SpritePresentationAdapter(transform, Renderer);
            Adapter.Initialize(new SpriteDefinition(definition.Id, PlaceholderSprite.Shared, Role), Role, this,
                feedback, definition.BodyMotion ? motion : null);
            IsRunning = true;
            Velocity = Vector2.zero;
        }

        public void Signal(PresentationSignal signal) => Signaled?.Invoke(signal);
        public void Tick(float deltaTime) => Adapter.Tick(deltaTime);
        public void Shutdown()
        {
            Adapter?.Shutdown();
            Adapter = null; IsRunning = false; Velocity = Vector2.zero;
        }
        private void OnDisable() => Shutdown();
        private void OnDestroy() => Shutdown();
    }
}
