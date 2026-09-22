using System.IO;
using Game.Content;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Editor
{
    public static class PresentationReviewCapture
    {
        // Static authoring diagnostic: pairs at the physical contact boundary.
        public static void CaptureContacts()
        {
            var sprites = FixtureSpriteCatalog.CreateFor(new ContentId[] {
                "FIXTURE-CHARACTER-AGILE-VISUAL-BODY", "FIXTURE-ENEMY-SEEKER-VISUAL" });
            var root = new GameObject("Contact boundary review");
            var preview = new PreviewRenderUtility();
            var materials = new[] { new Material(Shader.Find("Sprites/Default")),
                new Material(Shader.Find("Sprites/Default")), new Material(Shader.Find("Sprites/Default")) };
            for (var i = 0; i < 2; i++) materials[i].mainTexture = sprites[i].Sprite.texture;
            materials[2].mainTexture = Texture2D.whiteTexture;
            Texture2D image = null;
            try
            {
                for (var pair = 0; pair < 8; pair++)
                {
                    var origin = new Vector3(-2.7f + pair % 4 * 1.8f, .9f - pair / 4 * 1.8f, 0);
                    var angle = pair * Mathf.PI / 4;
                    var delta = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) *
                        (sprites[0].Contact.Radius + sprites[1].Contact.Radius);
                    for (var i = 0; i < 2; i++)
                    {
                        var actor = new GameObject("Body");
                        actor.transform.SetParent(root.transform, false);
                        actor.transform.localPosition = origin + (i == 0 ? Vector3.zero : delta);
                        var body = new GameObject("Visual");
                        body.transform.SetParent(actor.transform, false);
                        body.transform.localPosition = Vector3.down * sprites[i].Contact.CenterY;
                        var renderer = body.AddComponent<SpriteRenderer>();
                        renderer.sprite = sprites[i].Sprite;
                        renderer.sharedMaterial = materials[i];
                        renderer.flipX = pair >= 4;
                        renderer.sortingOrder = i;
                        var line = actor.AddComponent<LineRenderer>();
                        line.useWorldSpace = false;
                        line.loop = true;
                        line.positionCount = 64;
                        line.startWidth = line.endWidth = .012f;
                        line.sharedMaterial = materials[2];
                        line.startColor = line.endColor = i == 0 ? Color.cyan : Color.yellow;
                        line.sortingOrder = 10;
                        for (var p = 0; p < 64; p++)
                        {
                            var a = p * Mathf.PI * 2 / 64;
                            line.SetPosition(p, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * sprites[i].Contact.Radius);
                        }
                    }
                }
                preview.AddSingleGO(root);
                preview.camera.orthographic = true;
                preview.camera.orthographicSize = 2.6f;
                preview.camera.transform.position = new Vector3(0, 0, -10);
                preview.camera.nearClipPlane = .1f;
                preview.camera.farClipPlane = 30;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(.18f, .2f, .19f);
                preview.BeginStaticPreview(new Rect(0, 0, 1920, 1080));
                preview.Render(true);
                image = preview.EndStaticPreview();
                Directory.CreateDirectory("TestResults");
                File.WriteAllBytes("TestResults/body-contact-review.png", image.EncodeToPNG());
            }
            finally
            {
                if (image != null) Object.DestroyImmediate(image);
                preview.Cleanup();
                foreach (var material in materials) Object.DestroyImmediate(material);
            }
        }

        // Reproducible imported-sprite scale review; not a gameplay/dense-set acceptance.
        public static void CaptureVillager()
        {
            var sprites = FixtureSpriteCatalog.CreateFor(new ContentId[] {
                "FIXTURE-CHARACTER-AGILE-VISUAL-BODY", "FIXTURE-ENEMY-SEEKER-VISUAL" });
            var root = new GameObject("Villager scale review");
            var preview = new PreviewRenderUtility();
            var materials = new[] { new Material(Shader.Find("Sprites/Default")),
                new Material(Shader.Find("Sprites/Default")) };
            for (var i = 0; i < materials.Length; i++)
                materials[i].mainTexture = sprites[i].Sprite.texture;
            Texture2D image = null;
            try
            {
                for (var i = 0; i < 11; i++)
                {
                    var actor = new GameObject(i == 0 ? "Player reference" : "Villager");
                    actor.transform.SetParent(root.transform, false);
                    actor.transform.localPosition = i == 0 ? new Vector3(-3, 0, 0) :
                        new Vector3(-.5f + (i-1)%5 * 1.5f, (i-1)/5 * -2f, 0);
                    var renderer = actor.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprites[i == 0 ? 0 : 1].Sprite;
                    renderer.flipX = i % 2 == 0;
                    renderer.sharedMaterial = materials[i == 0 ? 0 : 1];
                }
                preview.AddSingleGO(root);
                preview.camera.orthographic = true;
                preview.camera.orthographicSize = 5;
                preview.camera.transform.position = new Vector3(0, 0, -10);
                preview.camera.nearClipPlane = .1f; preview.camera.farClipPlane = 30;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(.18f, .2f, .19f);
                preview.BeginStaticPreview(new Rect(0, 0, 1920, 1080));
                preview.Render(true);
                image = preview.EndStaticPreview();
                Directory.CreateDirectory("TestResults");
                File.WriteAllBytes("TestResults/enemy-001-scale-review.png", image.EncodeToPNG());
            }
            finally
            {
                if (image != null) Object.DestroyImmediate(image);
                preview.Cleanup();
                foreach (var material in materials) Object.DestroyImmediate(material);
            }
        }

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
