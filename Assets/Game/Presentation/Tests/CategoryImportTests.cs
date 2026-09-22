using System;
using System.IO;
using System.Linq;
using Game.Presentation.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class CategoryImportTests
    {
        private const string Body = "Assets/Resources/Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png";
        [TestCase("Skills", 16)]
        [TestCase("Passives", 14)]
        [TestCase("Sets", 20)]
        public void ApprovedGameplayIcons_AllUseTheIconImportProfile(string category, int expectedCount)
        {
            var directory = $"Assets/Resources/Art/UI/Icons/{category}";
            var paths = Directory.GetFiles(directory, "*.png").OrderBy(path => path).ToArray();
            Assert.AreEqual(expectedCount, paths.Length);
            foreach (var path in paths)
            {
                var assetPath = path.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                Assert.IsNotNull(importer, assetPath);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                Assert.AreEqual(TextureImporterType.Sprite, importer.textureType, assetPath);
                Assert.AreEqual(256, importer.maxTextureSize, assetPath);
                Assert.AreEqual(new Vector2(.5f, .5f), settings.spritePivot, assetPath);
                Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression, assetPath);
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath), assetPath);
            }
        }

        [TestCase("UI", "fixture-import-check-icon.png", 256)]
        [TestCase("VFX", "fixture-import-check-impact.png", 512)]
        public void RepresentativeImport_UIAndVfx_ReimportUsesCategoryAndPreservesGuid(string folder, string filename, int size)
        {
            var directory = "Assets/Resources/Art/" + folder;
            var createdDirectory = !AssetDatabase.IsValidFolder(directory);
            var path = directory + "/" + filename;
            Assert.IsFalse(File.Exists(path), "Never replace a preexisting asset for an import test.");
            try
            {
                if (createdDirectory) AssetDatabase.CreateFolder("Assets/Resources/Art", folder);
                // Temporary import specimen reuses approved bytes; it is never shipped as UI/VFX art.
                File.Copy(Body, path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var guid = AssetDatabase.AssetPathToGUID(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                Assert.AreEqual(new Vector2(.5f, .5f), settings.spritePivot);
                Assert.AreEqual(size, importer.maxTextureSize);
                Assert.AreEqual(guid, AssetDatabase.AssetPathToGUID(path));
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Sprite>(path));
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
                if (createdDirectory && Directory.Exists(directory) && Directory.GetFileSystemEntries(directory).Length == 0)
                    AssetDatabase.DeleteAsset(directory);
            }
        }

        [TestCase("UI/Icons/fixture-icon.png", "icon", 256)]
        [TestCase("UI/Portraits/fixture-portrait.png", "portrait", 512)]
        [TestCase("VFX/fixture-impact.png", "impact", 512)]
        [TestCase("Sprites/Pickups/fixture-pickup.png", "pickup", 256)]
        public void Category_ProfileHasCenteredPivotAndCategorySize(string suffix, string role, int size)
        {
            var path = SpriteImportProfileCatalog.Root + suffix;
            Assert.AreEqual(role, SpriteImportProfileCatalog.CategoryFor(path));
            var profile = SpriteImportProfileCatalog.Resolve(path);
            Assert.AreEqual(.5f, profile.PivotY);
            Assert.AreEqual(size, profile.MaxSize);
        }

        [Test]
        public void Body_ReimportRetainsExplicitGroundPivotAndGuid()
        {
            var guid = AssetDatabase.AssetPathToGUID(Body);
            AssetDatabase.ImportAsset(Body, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Body);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            Assert.AreEqual(new Vector2(.5f, .09f), settings.spritePivot);
            Assert.AreEqual(320, importer.spritePixelsPerUnit);
            Assert.AreEqual(guid, AssetDatabase.AssetPathToGUID(Body));
            Assert.Throws<InvalidOperationException>(() => SpriteImportProfileCatalog.Resolve(
                "Assets/Resources/Art/Sprites/Enemies/unreviewed-body.png"));
        }

        [Test]
        public void ExplicitProfile_InvalidOrUnrecordedValuesAreRejected()
        {
            Assert.Throws<InvalidOperationException>(() => new SpriteImportProfile { Key = "icon" }.Validate());
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpriteImportProfile
            { Key = "icon", Reason = "test", PixelsPerUnit = float.NaN, MaxSize = 256, PivotX = .5f, PivotY = .5f }.Validate());
            Assert.Throws<InvalidOperationException>(() => SpriteImportProfileCatalog.CategoryFor("Assets/Resources/Art/UI/fixture-body.png"));
        }

        [Test]
        public void ApprovedBody_AlphaBorderIsTransparentAndSilhouetteHasPadding()
        {
            var texture = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(texture.LoadImage(File.ReadAllBytes(Body)));
                var min = new Vector2Int(texture.width, texture.height); var max = Vector2Int.zero;
                for (var y = 0; y < texture.height; y++)
                for (var x = 0; x < texture.width; x++)
                {
                    if (texture.GetPixel(x, y).a == 0) continue;
                    min = Vector2Int.Min(min, new Vector2Int(x,y)); max = Vector2Int.Max(max, new Vector2Int(x,y));
                }
                Assert.GreaterOrEqual(min.x, 2); Assert.GreaterOrEqual(min.y, 2);
                Assert.Less(max.x, texture.width - 2); Assert.Less(max.y, texture.height - 2);
                Assert.Greater(max.y - min.y, texture.height / 2);
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
    }
}
