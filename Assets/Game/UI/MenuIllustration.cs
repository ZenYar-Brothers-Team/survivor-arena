using System;
using Game.Content;
using Game.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class MenuIllustration : IDisposable
    {
        private readonly VisualElement _root;
        private readonly Image _back, _front;
        private readonly MenuAtmosphereElement _rays, _dust;
        private readonly MenuArtProfile _profile;
        private readonly IVisualElementScheduledItem _tick;
        private Vector2 _target, _pointer;
        private double _last;
        private float _seconds;
        private bool _visible, _disposed;
        public float Seconds => _seconds;

        public MenuIllustration(VisualElement menu)
        {
            _profile = MenuArtProfile.Load();
            var layers = FixtureSpriteCatalog.CreateFor(new ContentId[] { _profile.backgroundId, _profile.foregroundId });
            layers[0].RequireRole(SpriteRole.Background); layers[1].RequireRole(SpriteRole.Background);
            _root = EntryUi.Box("entry-art"); _root.name = GameplayUiElementIds.EntryMenuArt; _root.pickingMode = PickingMode.Ignore;
            _back = EntryUi.Image(layers[0].Sprite, "entry-art-layer"); _back.AddToClassList("entry-art-back"); _back.scaleMode = ScaleMode.ScaleAndCrop;
            _front = EntryUi.Image(layers[1].Sprite, "entry-art-layer"); _front.AddToClassList("entry-art-front");
            _rays = new MenuAtmosphereElement(_profile, false); _dust = new MenuAtmosphereElement(_profile, true);
            _root.Add(_back); _root.Add(_rays); _root.Add(_front); _root.Add(_dust); menu.Insert(0, _root);
            menu.RegisterCallback<PointerMoveEvent>(Move); menu.RegisterCallback<PointerLeaveEvent>(Leave);
            _tick = _root.schedule.Execute(Tick).Every(33); _tick.Pause();
        }
        private void Move(PointerMoveEvent e)
        {
            var p = _root.WorldToLocal(e.position);
            _target = new Vector2(Mathf.Clamp(p.x / Mathf.Max(1, _root.contentRect.width) * 2 - 1, -1, 1),
                Mathf.Clamp(p.y / Mathf.Max(1, _root.contentRect.height) * 2 - 1, -1, 1));
        }
        private void Leave(PointerLeaveEvent e) => _target = Vector2.zero;
        public void SetVisible(bool visible)
        {
            if (_disposed || _visible == visible) return;
            _visible = visible; _last = Time.realtimeSinceStartupAsDouble;
            if (visible) _tick.Resume();
            else { _tick.Pause(); _target = _pointer = Vector2.zero; }
        }
        private void Tick()
        {
            var now = Time.realtimeSinceStartupAsDouble;
            var delta = Mathf.Clamp((float)(now - _last), 0, .1f); _last = now;
            if (!_visible || _disposed || !Application.isFocused) return;
            _seconds += delta;
            _pointer = Vector2.Lerp(_pointer, _target, 1 - Mathf.Exp(-_profile.pointerResponse * delta));
            var wave = Mathf.Sin(_seconds * Mathf.PI * 2 / _profile.driftSeconds);
            Pose(_back, _profile.backgroundDrift, _profile.backgroundPointer, wave);
            Pose(_front, _profile.foregroundDrift, _profile.foregroundPointer, wave);
            _rays.Seconds = _dust.Seconds = _seconds; _rays.MarkDirtyRepaint(); _dust.MarkDirtyRepaint();
        }
        private void Pose(Image image, float drift, float pointer, float wave)
        {
            // Animated offsets are runtime values; static geometry and appearance stay in USS.
            image.style.translate = new Translate((_pointer.x * pointer + wave * drift) * _root.contentRect.width,
                (_pointer.y * pointer * .625f - wave * drift * .5f) * _root.contentRect.height);
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; _tick.Pause();
            _root.parent?.UnregisterCallback<PointerMoveEvent>(Move); _root.parent?.UnregisterCallback<PointerLeaveEvent>(Leave);
            _root.RemoveFromHierarchy();
        }
    }
}
