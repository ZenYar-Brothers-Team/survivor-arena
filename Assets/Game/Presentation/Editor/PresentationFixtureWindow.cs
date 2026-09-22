using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Editor
{
    // A separate, opt-in Editor diagnostic view; never overlays the gameplay viewport or ships in builds.
    public sealed class PresentationFixtureWindow : EditorWindow
    {
        private PresentationFixtureKit _kit;
        private PreviewRenderUtility _preview;
        private SpritePortraitCrop _crop;
        private double _lastTime;
        private Vector2 _scroll;

        [MenuItem("Tools/Survivor Arena/Presentation Fixture Review")]
        private static void Open() => GetWindow<PresentationFixtureWindow>("Presentation Review");

        private void OnEnable()
        {
            try
            {
                _kit = new PresentationFixtureKit();
                _preview = new PreviewRenderUtility();
                _preview.AddSingleGO(_kit.Root);
                _preview.camera.orthographic = true;
                _preview.camera.orthographicSize = 5;
                _preview.camera.transform.position = new Vector3(0, 0, -10);
                _preview.camera.nearClipPlane = .1f;
                _preview.camera.farClipPlane = 30;
                var body = FixtureSpriteCatalog.CreateFor(new Game.Content.ContentId[] { "FIXTURE-CHARACTER-AGILE-VISUAL-BODY" })[0];
                _crop = new SpritePortraitCrop("FIXTURE-CHARACTER-AGILE-VISUAL-PORTRAIT", body, new Rect(0, 0, 1, 1));
                _lastTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += Advance;
            }
            catch { OnDisable(); throw; }
        }
        private void Advance()
        {
            var now = EditorApplication.timeSinceStartup;
            _kit?.Tick((float)(now - _lastTime));
            _lastTime = now;
            Repaint();
        }
        private void OnGUI()
        {
            if (_kit == null || _preview == null) return;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.HelpBox("Synthetic shapes test roles, pause and pool reuse. They are not approved production art. " +
                "Four copies test adapter overlap, not a full gameplay density review.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Idle")) _kit.SetVelocity(Vector2.zero);
            if (GUILayout.Button("Left")) _kit.SetVelocity(Vector2.left);
            if (GUILayout.Button("Right")) _kit.SetVelocity(Vector2.right);
            if (GUILayout.Button(_kit.IsRunning ? "Pause" : "Resume")) _kit.SetRunning(!_kit.IsRunning);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            foreach (var signal in new[] { PresentationSignal.Hit, PresentationSignal.Proc, PresentationSignal.Death, PresentationSignal.Collect })
                if (GUILayout.Button(signal.ToString())) _kit.Signal(signal);
            if (GUILayout.Button("Reset")) _kit.Reset(1);
            if (GUILayout.Button("4 copies")) _kit.Reset(4);
            EditorGUILayout.EndHorizontal();
            var rect = GUILayoutUtility.GetRect(100, 360, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                _preview.BeginPreview(rect, GUIStyle.none);
                _preview.Render(true);
                _preview.EndAndDrawPreview(rect);
            }
            EditorGUILayout.LabelField("Body reuse in a centered UI slot (preview; visual review pending)");
            var slot = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(slot, _crop.Definition.Sprite.texture, ScaleMode.ScaleToFit, true);
            EditorGUILayout.EndScrollView();
        }
        private void OnDisable()
        {
            EditorApplication.update -= Advance;
            _kit?.Dispose(); _kit = null;
            _crop?.Dispose(); _crop = null;
            _preview?.Cleanup(); _preview = null;
        }
    }
}
