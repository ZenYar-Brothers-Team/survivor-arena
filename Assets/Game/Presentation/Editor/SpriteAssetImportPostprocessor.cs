using System;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Editor
{
    public sealed class SpriteAssetImportPostprocessor : AssetPostprocessor
    {
        private const string RuntimeSpriteRoot = "Assets/Resources/Art/Sprites/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(RuntimeSpriteRoot, StringComparison.Ordinal))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 320f;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = assetPath.EndsWith("-body.png", StringComparison.Ordinal)
                ? new Vector2(0.5f, 0.09f)
                : new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
        }
    }
}
