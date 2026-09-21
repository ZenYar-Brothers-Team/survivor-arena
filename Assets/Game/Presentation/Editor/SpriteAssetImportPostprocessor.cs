using System;
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.Editor
{
    public sealed class SpriteAssetImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(SpriteImportProfileCatalog.Root, StringComparison.Ordinal)) return;
            Apply((TextureImporter)assetImporter, SpriteImportProfileCatalog.Resolve(assetPath));
        }

        public static void Apply(TextureImporter importer, SpriteImportProfile profile)
        {
            profile.Validate();
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = profile.PixelsPerUnit.Value;
            importer.maxTextureSize = profile.MaxSize.Value;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 1;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(profile.PivotX.Value, profile.PivotY.Value);
            importer.SetTextureSettings(settings);
        }
    }
}
