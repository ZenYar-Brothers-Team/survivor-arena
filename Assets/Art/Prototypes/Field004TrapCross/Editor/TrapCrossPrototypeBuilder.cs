using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.ArtPrototypes.Editor
{
    /// <summary>Editor-only art experiment. No gameplay components, colliders or production bindings.</summary>
    public static class TrapCrossPrototypeBuilder
    {
        private const string AssetRoot = "Assets/Art/Prototypes/Field004TrapCross";
        private const string SourcePath = "Art/Prototypes/field004-trap-cross/model.json";
        private const string PreviewRoot = "Art/Prototypes/field004-trap-cross/preview";

        [MenuItem("Tools/Survivor Arena/Art Prototypes/Build 3D Trap Cross")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Build this isolated art prototype outside Play Mode.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var config = JObject.Parse(File.ReadAllText(SourcePath));
            Directory.CreateDirectory(AssetRoot + "/Materials");
            Directory.CreateDirectory(AssetRoot + "/Meshes");
            Directory.CreateDirectory(PreviewRoot);
            AssetDatabase.Refresh();
            var shader = Shader.Find("ArtPrototypes/TrapCrossMatte");
            if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Prototype shader unavailable or invalid.");
            var materials = new Dictionary<string, Material>();
            foreach (var property in ((JObject)config["materials"]).Properties())
            {
                var path = AssetRoot + "/Materials/" + property.Name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
                material.shader = shader;
                var color = (JArray)property.Value;
                material.SetColor("_Color", new Color((float)color[0], (float)color[1], (float)color[2], 1f));
                material.SetFloat("_Expansion", property.Name == "outline" ? (float)config["outlineWidth"] : 0f);
                material.SetFloat("_Cull", property.Name == "outline" ? 1f : 2f);
                material.SetFloat("_Unlit", 1f);
                material.SetFloat("_MaterialKind", property.Name.StartsWith("wood", StringComparison.Ordinal) ? 1f : property.Name == "outline" ? 0f : 2f);
                EditorUtility.SetDirty(material);
                materials.Add(property.Name, material);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("TRAP-002-3D-Prototype");
            var stationary = new GameObject("StationaryBase").transform;
            stationary.SetParent(root.transform, false);
            var head = new GameObject("RotatingHead").transform;
            head.SetParent(root.transform, false);
            head.localPosition = Vector(config["headPivot"]);
            var meshCache = new Dictionary<string, Mesh>();
            foreach (var token in (JArray)config["parts"])
            {
                var part = (JObject)token;
                var parent = (string)part["group"] == "head" ? head : stationary;
                var node = new GameObject((string)part["name"]);
                node.transform.SetParent(parent, false);
                node.transform.localPosition = Vector(part["position"]);
                node.transform.localRotation = Quaternion.Euler(Vector(part["rotation"]));
                node.transform.localScale = Vector(part["scale"]);
                var shape = (string)part["shape"];
                if (!meshCache.TryGetValue(shape, out var mesh))
                {
                    mesh = CreateMesh(shape, (int)config["radialSegments"]);
                    mesh.name = "trap-cross-" + shape;
                    var meshPath = AssetRoot + "/Meshes/" + mesh.name + ".asset";
                    var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                    if (existing == null) AssetDatabase.CreateAsset(mesh, meshPath);
                    else { EditorUtility.CopySerialized(mesh, existing); UnityEngine.Object.DestroyImmediate(mesh); mesh = existing; }
                    meshCache.Add(shape, mesh);
                }
                var filter = node.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                var renderer = node.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials[(string)part["material"]];
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                var outline = new GameObject("Contour");
                outline.transform.SetParent(node.transform, false);
                outline.AddComponent<MeshFilter>().sharedMesh = mesh;
                var contourRenderer = outline.AddComponent<MeshRenderer>();
                contourRenderer.sharedMaterial = materials["outline"];
                contourRenderer.shadowCastingMode = ShadowCastingMode.Off;
                contourRenderer.receiveShadows = false;
            }
            if (root.GetComponentsInChildren<Collider>().Length != 0 || root.GetComponentsInChildren<Collider2D>().Length != 0)
                throw new InvalidOperationException("Art-only prototype must have no physics.");
            PrefabUtility.SaveAsPrefabAsset(root, AssetRoot + "/trap-002-cross.prefab");

            var camera = new GameObject("PrototypeCamera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = (float)config["cameraSize"];
            camera.transform.position = Vector(config["cameraPosition"]);
            camera.transform.LookAt(Vector(config["cameraTarget"]));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.14f, 0.106f, 0.169f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
            EditorSceneManager.SaveScene(scene, AssetRoot + "/trap-002-cross-review.unity");
            AssetDatabase.SaveAssets();
            Capture(config, root, head, camera);
            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("TRAP CROSS PROTOTYPE COMPLETE: " + AssetRoot + "; preview: " + PreviewRoot);
        }

        private static void Capture(JObject config, GameObject root, Transform head, Camera camera)
        {
            var count = (int)config["frameCount"];
            var size = (int)config["renderSize"];
            var target = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var pixels = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var positions = new Vector3[root.transform.Find("StationaryBase").childCount];
            for (var i = 0; i < positions.Length; i++) positions[i] = root.transform.Find("StationaryBase").GetChild(i).position;
            var firstSpear = head.Find("Arm-0-Tip");
            var anchor = head.position;
            var distance = Vector3.Distance(anchor, firstSpear.position);
            try
            {
                for (var frame = 0; frame < count; frame++)
                {
                    head.localRotation = Quaternion.Euler(0f, frame * 360f / count, 0f);
                    if (Vector3.Distance(head.position, anchor) > 0.00001f || Math.Abs(Vector3.Distance(anchor, firstSpear.position) - distance) > 0.00001f)
                        throw new InvalidOperationException("Head rotation changed its attachment or radius.");
                    for (var i = 0; i < positions.Length; i++)
                        if (Vector3.Distance(positions[i], root.transform.Find("StationaryBase").GetChild(i).position) > 0.00001f)
                            throw new InvalidOperationException("Rotating head moved the stationary base.");
                    if (GraphicsSettings.currentRenderPipeline != null)
                        RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                    else { camera.targetTexture = target; camera.Render(); }
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                    pixels.Apply();
                    File.WriteAllBytes(PreviewRoot + "/frame-" + frame.ToString("D3") + ".png", pixels.EncodeToPNG());
                }
                File.WriteAllText(PreviewRoot + "/verification.json", new JObject
                {
                    ["unityVersion"] = Application.unityVersion,
                    ["frames"] = count,
                    ["headAxis"] = "local Y, genuine 3D rotation",
                    ["stationaryBaseUnchanged"] = true,
                    ["headAttachmentUnchanged"] = true,
                    ["spearOrbitRadiusUnchanged"] = true,
                    ["physicsComponentCount"] = 0,
                    ["meshParts"] = ((JArray)config["parts"]).Count,
                    ["scene"] = AssetRoot + "/trap-002-cross-review.unity",
                    ["prefab"] = AssetRoot + "/trap-002-cross.prefab"
                }.ToString());
            }
            finally
            {
                head.localRotation = Quaternion.identity;
                camera.targetTexture = null;
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(pixels);
            }
        }

        private static Vector3 Vector(JToken token)
        {
            return new Vector3((float)token[0], (float)token[1], (float)token[2]);
        }

        private static Mesh CreateMesh(string shape, int segments)
        {
            if (shape == "box")
            {
                var temporary = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var mesh = UnityEngine.Object.Instantiate(temporary.GetComponent<MeshFilter>().sharedMesh);
                UnityEngine.Object.DestroyImmediate(temporary);
                return mesh;
            }
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            if (shape == "cylinder")
            {
                for (var i = 0; i < segments; i++)
                {
                    var a = i * Mathf.PI * 2f / segments;
                    var b = (i + 1) * Mathf.PI * 2f / segments;
                    var lowerA = new Vector3(Mathf.Cos(a) * .5f, -.5f, Mathf.Sin(a) * .5f);
                    var lowerB = new Vector3(Mathf.Cos(b) * .5f, -.5f, Mathf.Sin(b) * .5f);
                    var upperA = lowerA + Vector3.up;
                    var upperB = lowerB + Vector3.up;
                    Triangle(vertices, triangles, lowerA, upperA, upperB);
                    Triangle(vertices, triangles, lowerA, upperB, lowerB);
                    Triangle(vertices, triangles, Vector3.up * .5f, upperB, upperA);
                    Triangle(vertices, triangles, Vector3.down * .5f, lowerA, lowerB);
                }
            }
            else if (shape == "spear")
            {
                var tip = new Vector3(.5f, 0f, 0f);
                var rear = new Vector3(-.5f, 0f, 0f);
                var left = new Vector3(-.12f, 0f, -.5f);
                var right = new Vector3(-.12f, 0f, .5f);
                var top = new Vector3(-.12f, .5f, 0f);
                var bottom = new Vector3(-.12f, -.5f, 0f);
                Triangle(vertices, triangles, tip, top, right);
                Triangle(vertices, triangles, tip, left, top);
                Triangle(vertices, triangles, rear, right, top);
                Triangle(vertices, triangles, rear, top, left);
                Triangle(vertices, triangles, tip, right, bottom);
                Triangle(vertices, triangles, tip, bottom, left);
                Triangle(vertices, triangles, rear, bottom, right);
                Triangle(vertices, triangles, rear, left, bottom);
            }
            else throw new ArgumentException("Unsupported prototype mesh: " + shape);
            var result = new Mesh();
            result.SetVertices(vertices);
            result.SetTriangles(triangles, 0);
            result.RecalculateNormals();
            result.RecalculateBounds();
            return result;
        }

        private static void Triangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            var index = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
        }
    }
}
