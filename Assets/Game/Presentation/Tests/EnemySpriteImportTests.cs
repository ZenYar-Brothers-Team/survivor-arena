using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class EnemySpriteImportTests
    {
        private const string Path = "Assets/Resources/Art/Sprites/Enemies/enemy-001/enemy-001-body.png";

        [Test]
        public void Villager_ImportAndReimport_PreserveSizeAlphaPivotAndGuid()
        {
            var guid = AssetDatabase.AssetPathToGUID(Path);
            AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceUpdate);
            Assert.AreEqual(guid, AssetDatabase.AssetPathToGUID(Path));
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Path);
            Assert.IsNotNull(Resources.Load<Sprite>("Art/Sprites/Enemies/enemy-001/enemy-001-body"));
            Assert.AreEqual(new Vector2(256, 256), sprite.rect.size);
            Assert.AreEqual(new Vector2(.53f, .1f), importer.spritePivot);
            Assert.AreEqual(160, importer.spritePixelsPerUnit);
            Assert.AreEqual(256, importer.maxTextureSize);
            Assert.AreEqual(TextureImporterType.Sprite, importer.textureType);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            Assert.IsTrue(importer.alphaIsTransparency);
            Assert.IsFalse(importer.mipmapEnabled);
            Assert.IsFalse(importer.isReadable);
            var texture = new Texture2D(2, 2);
            try
            {
                texture.LoadImage(File.ReadAllBytes(Path));
                for (var x = 0; x < 256; x++)
                    for (var edge = 0; edge < 2; edge++)
                    {
                        Assert.AreEqual(0, texture.GetPixel(x, edge).a);
                        Assert.AreEqual(0, texture.GetPixel(x, 255-edge).a);
                        Assert.AreEqual(0, texture.GetPixel(edge, x).a);
                        Assert.AreEqual(0, texture.GetPixel(255-edge, x).a);
                    }
            }
            finally { Object.DestroyImmediate(texture); }
        }
    }
}
