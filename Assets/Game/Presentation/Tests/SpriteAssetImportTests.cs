using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class SpriteAssetImportTests
    {
        [Test]
        public void FolioSurface_UsesWholeMatteCanvas()
        {
            const string path = "Assets/Resources/Art/UI/Folio/ui-folio-surface-background.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Assert.IsNotNull(importer);
            Assert.IsNotNull(sprite);
            Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode);
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.AreEqual(TextureWrapMode.Clamp, importer.wrapMode);
            Assert.AreEqual(FilterMode.Bilinear, importer.filterMode);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            Assert.AreEqual(sprite.texture.width, sprite.rect.width);
            Assert.AreEqual(sprite.texture.height, sprite.rect.height);
        }

        [TestCase("ui-menu-back-background")]
        [TestCase("ui-menu-front-background")]
        public void MenuLayer_PreservesWholeCanvas(string name)
        {
            var path = "Assets/Resources/Art/UI/Menu/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Assert.AreEqual(sprite.texture.width, sprite.rect.width);
            Assert.AreEqual(sprite.texture.height, sprite.rect.height);
        }
        private const string AssetPath =
            "Assets/Resources/Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png";
        private const string ResourcePath =
            "Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body";

        [Test]
        public void AgileGoblinBody_FollowsRuntimeSpriteImportContract()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetPath);
            var importer = AssetImporter.GetAtPath(AssetPath) as TextureImporter;

            Assert.IsNotNull(sprite);
            Assert.IsNotNull(importer);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Assert.AreEqual(512f, sprite.rect.width);
            Assert.AreEqual(512f, sprite.rect.height);
            Assert.AreEqual(TextureImporterType.Sprite, importer.textureType);
            Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode);
            Assert.IsTrue(importer.sRGBTexture);
            Assert.AreEqual(TextureImporterAlphaSource.FromInput, importer.alphaSource);
            Assert.IsTrue(importer.alphaIsTransparency);
            Assert.IsFalse(importer.isReadable);
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.AreEqual(TextureWrapMode.Clamp, importer.wrapMode);
            Assert.AreEqual(FilterMode.Bilinear, importer.filterMode);
            Assert.AreEqual(SpriteMeshType.FullRect, settings.spriteMeshType);
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(426.666667f).Within(.001f));
            Assert.AreEqual((int)SpriteAlignment.Custom, settings.spriteAlignment);
            Assert.AreEqual(new Vector2(0.5f, 0.09f), settings.spritePivot);
            Assert.AreEqual(512, importer.maxTextureSize);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            Assert.IsFalse(importer.crunchedCompression);
            Assert.IsNotNull(Resources.Load<Sprite>(ResourcePath));
            Assert.IsTrue(File.Exists(
                "Art/Source/Characters/fixture-character-agile/asset-record.json"));
        }

        [TestCase("Assets/Resources/Art/Sprites/Pickups/xp-pickup/xp-pickup.png")]
        [TestCase("Assets/Resources/Art/Sprites/Pickups/pickup-001/pickup-001-pickup.png")]
        [TestCase("Assets/Resources/Art/Sprites/Pickups/traveler-book/traveler-book-pickup.png")]
        public void Pickup_FollowsRuntimeSpriteImportContract(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.IsNotNull(sprite);
            Assert.IsNotNull(importer);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Assert.AreEqual(256f, sprite.rect.width);
            Assert.AreEqual(256f, sprite.rect.height);
            Assert.AreEqual(TextureImporterType.Sprite, importer.textureType);
            Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode);
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.AreEqual(FilterMode.Bilinear, importer.filterMode);
            Assert.AreEqual(TextureWrapMode.Clamp, importer.wrapMode);
            Assert.AreEqual(SpriteMeshType.FullRect, settings.spriteMeshType);
            Assert.AreEqual(320f, importer.spritePixelsPerUnit);
            Assert.AreEqual(new Vector2(.5f, .5f), settings.spritePivot);
            Assert.AreEqual(256, importer.maxTextureSize);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
        }
    }
}
