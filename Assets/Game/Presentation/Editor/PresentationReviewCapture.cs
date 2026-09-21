using System.IO;
using Game.Content;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Editor
{
    public static class PresentationReviewCapture
    {
        [MenuItem("Tools/Survivor Arena/Capture Presentation Fixture Review")]
        public static void Capture()
        {
            using var kit = new PresentationFixtureKit();
            var preview = new PreviewRenderUtility();
            Texture2D image = null;
            Material spriteMaterial = null;
            try
            {
                kit.Reset(4);
                kit.SetVelocity(Vector2.left);
                kit.Signal(PresentationSignal.Proc);
                kit.Tick(.03f);
                var body = FixtureSpriteCatalog.CreateFor(new ContentId[] { "FIXTURE-CHARACTER-AGILE-VISUAL-BODY" })[0];
                var player = new GameObject("Approved body reference");
                player.transform.SetParent(kit.Root.transform, false);
                player.transform.localPosition = new Vector3(0, 2, 0);
                var renderer = player.AddComponent<SpriteRenderer>();
                renderer.sprite = body.Sprite;
                spriteMaterial = new Material(Shader.Find("Sprites/Default"));
                renderer.sharedMaterial = spriteMaterial;
                preview.AddSingleGO(kit.Root);
                preview.camera.orthographic = true;
                preview.camera.orthographicSize = 5;
                preview.camera.transform.position = new Vector3(0, 0, -10);
                preview.camera.nearClipPlane = .1f; preview.camera.farClipPlane = 30;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(.12f, .14f, .18f);
                preview.BeginStaticPreview(new Rect(0, 0, 1920, 1080));
                preview.Render(true);
                image = preview.EndStaticPreview();
                Directory.CreateDirectory("TestResults");
                File.WriteAllBytes("TestResults/presentation-ip12a.png", image.EncodeToPNG());
            }
            finally
            {
                if (image != null) Object.DestroyImmediate(image);
                if (spriteMaterial != null) Object.DestroyImmediate(spriteMaterial);
                preview.Cleanup();
            }
        }
    }
}
