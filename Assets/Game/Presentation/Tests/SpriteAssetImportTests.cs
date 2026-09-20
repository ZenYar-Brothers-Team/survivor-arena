using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class SpriteAssetImportTests
    {
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
            Assert.AreEqual(320f, importer.spritePixelsPerUnit);
            Assert.AreEqual((int)SpriteAlignment.Custom, settings.spriteAlignment);
            Assert.AreEqual(new Vector2(0.5f, 0.09f), settings.spritePivot);
            Assert.AreEqual(512, importer.maxTextureSize);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            Assert.IsFalse(importer.crunchedCompression);
            Assert.IsNotNull(Resources.Load<Sprite>(ResourcePath));
            Assert.IsTrue(File.Exists(
                "Art/Source/Characters/fixture-character-agile/asset-record.json"));
        }
    }
}
