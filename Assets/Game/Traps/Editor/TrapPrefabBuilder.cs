using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Game.Traps.Editor
{
    /// <summary>
    /// Builds the 3D trap prefabs from the Blender FBX files in Assets/Art/Traps (one per trap, named trap-NNN.fbx) and lists them
    /// in the trap prefab library by that name (DECISION-0156). An FBX carries geometry, hierarchy and Blender material names
    /// only; each name is mapped onto the project's TrapCrossMatte materials, so every model keeps the painted wood and iron look
    /// of the 3D reference. Batch: <c>Unity -batchmode -projectPath . -executeMethod Game.Traps.Editor.TrapPrefabBuilder.Build -quit</c>.
    /// </summary>
    public static class TrapPrefabBuilder
    {
        private const string ModelFolder = "Assets/Art/Traps";
        private const string MaterialFolder = ModelFolder + "/Materials";
        private const string SourceMaterials = "Assets/Art/Prototypes/Field004TrapCross/Materials";
        private const string BlenderMaterialPrefix = "CrossBlender_";
        private const string HubSteel = "hub-steel";
        private const string Outline = "outline";

        // Blender revision 3 darkens only the hub top of the cross; its pigment is the steel kind with this colour.
        private static readonly Color HubSteelColor = new Color(.48f, .51f, .55f, 1f);

        // Spear and bolt tips use the two tones of the projectile sprites, so the loaded and the fired spear match.
        private static readonly Color TipLightColor = new Color(.60f, .60f, .62f, 1f);
        private static readonly Color TipDarkColor = new Color(.66f, .66f, .69f, 1f); // the second side: a touch lighter than TipLightColor, clear of the iron

        [MenuItem("Tools/Survivor Arena/Build Trap Prefabs (Blender)")]
        public static void Build()
        {
            var fbxFiles = Directory.GetFiles(ModelFolder, "trap-*.fbx").Select(f => f.Replace('\\', '/')).OrderBy(f => f).ToArray();
            if (fbxFiles.Length == 0) throw new InvalidOperationException($"No trap FBX files in {ModelFolder}.");
            Directory.CreateDirectory(MaterialFolder);
            var materials = LoadMaterials();
            var built = new List<TrapPrefabEntry>();
            foreach (var fbxPath in fbxFiles)
            {
                var key = Path.GetFileNameWithoutExtension(fbxPath);
                built.Add(new TrapPrefabEntry { key = key, prefab = BuildPrefab(fbxPath, key, materials) });
            }
            WriteLibrary(built);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"TRAP PREFABS BUILT: {built.Count} ({string.Join(", ", built.Select(e => e.key))}).");
        }

        private static GameObject BuildPrefab(string fbxPath, string key, Dictionary<string, Material> materials)
        {
            // The FBX keeps Blender's Z-up axes; baking the conversion gives an identity-rotated, Y-up, unit-scale model.
            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceSynchronousImport);
            if (AssetImporter.GetAtPath(fbxPath) is not ModelImporter importer)
                throw new InvalidOperationException($"{fbxPath} is not imported as a model.");
            if (!importer.bakeAxisConversion)
            {
                importer.bakeAxisConversion = true;
                importer.SaveAndReimport();
            }
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath) ?? throw new InvalidOperationException($"Cannot load {fbxPath}.");
            var instance = Object.Instantiate(fbx);
            try
            {
                instance.name = key;
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                {
                    var slots = renderer.sharedMaterials;
                    for (var i = 0; i < slots.Length; i++)
                    {
                        var name = slots[i] != null ? slots[i].name : string.Empty;
                        if (!name.StartsWith(BlenderMaterialPrefix, StringComparison.Ordinal))
                            throw new InvalidOperationException($"{key}/{renderer.name}: unexpected material '{name}'.");
                        var material = name.Substring(BlenderMaterialPrefix.Length);
                        slots[i] = materials.TryGetValue(material, out var mapped)
                            ? mapped : throw new InvalidOperationException($"{key}/{renderer.name}: no Unity material for '{material}'.");
                    }
                    renderer.sharedMaterials = slots;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                if (instance.transform.Find("RotatingHead") == null || instance.transform.Find("StationaryBase") == null)
                    throw new InvalidOperationException($"{key}: the model must have StationaryBase and RotatingHead under its root.");
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, $"{ModelFolder}/{key}.prefab") ??
                    throw new InvalidOperationException($"Could not save the prefab of {key}.");
                return prefab;
            }
            finally { Object.DestroyImmediate(instance); }
        }

        private static Dictionary<string, Material> LoadMaterials()
        {
            var map = new Dictionary<string, Material>(StringComparer.Ordinal);
            foreach (var name in new[] { "wood", "wood-light", "iron", "steel", "bronze" })
                map[name] = AssetDatabase.LoadAssetAtPath<Material>($"{SourceMaterials}/{name}.mat") ??
                    throw new InvalidOperationException($"Missing material {SourceMaterials}/{name}.mat");
            map[HubSteel] = CreateMaterial(HubSteel, map["steel"], HubSteelColor, null, null);
            map["tip-light"] = CreateMaterial("tip-light", map["steel"], TipLightColor, null, null);
            map["tip-dark"] = CreateMaterial("tip-dark", map["steel"], TipDarkColor, null, null);
            // The Blender contour is real geometry (an expanded shell with flipped faces), so it is culled like a normal back face,
            // with no shader expansion (that tears the shell at hard edges); the export scripts already set its thickness.
            var outline = AssetDatabase.LoadAssetAtPath<Material>($"{SourceMaterials}/{Outline}.mat") ??
                throw new InvalidOperationException("Missing outline material.");
            map[Outline] = CreateMaterial("outline-shell", outline, outline.GetColor("_Color"), 0f, 2f);
            return map;
        }

        private static Material CreateMaterial(string name, Material source, Color color, float? expansion, float? cull)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(source) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.CopyPropertiesFromMaterial(source);
            material.SetColor("_Color", color);
            if (expansion.HasValue) material.SetFloat("_Expansion", expansion.Value);
            if (cull.HasValue) material.SetFloat("_Cull", cull.Value);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void WriteLibrary(List<TrapPrefabEntry> entries)
        {
            var library = AssetDatabase.LoadAssetAtPath<TrapPrefabLibrary>("Assets/Resources/" + TrapPrefabLibrary.ResourcePath + ".asset") ??
                throw new InvalidOperationException("Missing TrapPrefabLibrary asset.");
            library.entries = entries.ToArray();
            EditorUtility.SetDirty(library);
        }
    }
}
