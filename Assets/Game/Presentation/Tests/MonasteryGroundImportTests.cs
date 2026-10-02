using Game.Presentation.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class MonasteryGroundImportTests
    {
        [Test]
        public void ApprovedGround_FullResolutionPreviewPreservesEightUnitRepeat()
        {
            const string path = "Assets/Resources/Art/Sprites/Fields/field-007/field-007-ground-tile.png";
            // Explicit reimport applies the authored profile; changing a file outside Assets does not trigger it.
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var profile = SpriteImportProfileCatalog.Resolve(path);
            Assert.AreEqual(profile.MaxSize.Value, importer.maxTextureSize);
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Assert.AreEqual(1254, sprite.texture.width);
            Assert.AreEqual(1254, sprite.texture.height);
            Assert.AreEqual(8f, sprite.bounds.size.x, .001f);
            Assert.AreEqual(8f, sprite.bounds.size.y, .001f);
        }
    }
}
