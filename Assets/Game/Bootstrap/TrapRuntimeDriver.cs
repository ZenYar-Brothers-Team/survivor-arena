using System;
using System.Collections.Generic;
using Game.Content;
using Game.Presentation;
using Game.Run;
using Game.Traps;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Game.Bootstrap
{
    /// <summary>
    /// Scene side of the traps (DECISION-0156): ticks <see cref="TrapRuntime"/> only while the run is Running and draws it.
    /// Turrets whose type names a 3D model are drawn with that prefab: tilted towards the camera, pushed forward in depth, the
    /// head turned to the simulated pose; a model is created when the player first comes near and hidden again when far.
    /// Projectiles and barrels use the sprites the layout names (by projectile visual key and "barrel"); anything without a model
    /// or sprite falls back to a plain rectangle. Turrets and barrels get a player-only physical body; everything here damages
    /// the player alone.
    /// </summary>
    public sealed class TrapRuntimeDriver : MonoBehaviour
    {
        private const int RingSortingOrder = 4;
        private const int ThreatSortingOrder = 5;
        private const int LineSortingOrder = 6;
        private const int BodySortingOrder = 8;
        private const int ProjectileSortingOrder = 12;
        private const float ExplosionFlashSeconds = 0.35f;
        private const float LineWidth = 0.18f;
        private const float DirectionalLengthFactor = 3.2f;
        private const float MinDirectionalLength = 0.5f;
        private const float RoundSpriteFactor = 1.3f;
        private const float BarrelSpriteFactor = 1.25f;
        private const float ModelRingFactor = 2.8f;
        // A model is made when the player is within this many radii and hidden beyond the larger one (no flicker at the edge).
        private const float ModelShowRadii = 1.3f;
        private const float ModelHideRadii = 1.6f;
        private const float HeadTurnDegreesPerSecond = 540f;
        // How far in front of the deepest model a projectile is drawn (a model reaches about one unit nearer than its offset).
        private const float ProjectileDepthMargin = 2f;

        private static readonly int ExpansionProperty = Shader.PropertyToID("_Expansion");
        private static readonly Color TurretColor = new Color(.2f, .45f, .85f, 1f);
        private static readonly Color TurretHeadColor = new Color(.65f, .82f, 1f, 1f);
        private static readonly Color RageColor = new Color(.95f, .2f, .15f, 1f);
        private static readonly Color ProjectileColor = new Color(1f, .85f, .1f, 1f);
        // A thin bright red outline hugs every trap projectile (user request 2026-10-03) to draw the eye to the danger: the sprite's own
        // silhouette dilated by a few screen pixels in a flat colour by the SpriteDilatedOutline shader, drawn behind the projectile.
        private static readonly Color ProjectileOutlineColor = new Color(1f, .05f, .04f, .95f);
        private const float ProjectileOutlineWidthPixels = 2.5f;
        // Placeholder rectangles have no canvas margin to dilate into, so their outline is the same square a little larger.
        private const float PlaceholderOutlineScale = 1.3f;

        private static readonly Color BarrelColor = new Color(.5f, .33f, .16f, 1f);

        private sealed class TurretView
        {
            public TrapPlacement Placement;
            public Transform Root;
            public SpriteRenderer Body;
            public SpriteRenderer Head;
            public SpriteRenderer Line;
            public SpriteRenderer Ring;
            public GameObject Model;
            public Transform ModelHead;
            public float HeadAngle;
        }

        private sealed class BarrelView
        {
            public TrapBarrelPlacement Placement;
            public SpriteRenderer Body;
            public SpriteRenderer Threat;
            public Collider2D Contact;
        }

        private readonly List<TurretView> _turrets = new List<TurretView>();
        private readonly List<BarrelView> _barrels = new List<BarrelView>();
        private readonly List<SpriteRenderer> _projectiles = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _projectileHalos = new List<SpriteRenderer>();
        private Material _outlineMaterial;
        // One outline material per sprite, with that sprite's texture set directly: a single shared material relied on a per-renderer
        // texture override, which let one projectile kind briefly borrow another kind's outline when renderers changed sprite.
        private readonly Dictionary<Sprite, Material> _outlineMaterials = new Dictionary<Sprite, Material>();
        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private TrapRuntime _runtime;
        private TrapLayoutDefinition _layout;
        private TrapPrefabLibrary _library;
        private ITrapPlayerTarget _player;
        private RunController _run;
        private Func<Rect> _view;
        private Sprite _square;
        private bool _cleared;
        // Models are pushed towards the camera in depth so they cover the ground sprites; projectiles must be drawn nearer still, or the
        // model would hide them (sprites do not sort against mesh depth).
        private float _projectileDepth;

        public TrapRuntime Runtime => _runtime;

        public static TrapRuntimeDriver Create(TrapLayoutDefinition layout, TrapPlacementSet placements,
            IReadOnlyList<IReadOnlyList<Vector2>> obstacleOutlines, float arenaSideLength, ITrapPlayerTarget player,
            RunController run, Scene scene, Func<Rect> view, ContentRegistry registry)
        {
            var root = new GameObject("FieldTraps");
            SceneManager.MoveGameObjectToScene(root, scene);
            var driver = root.AddComponent<TrapRuntimeDriver>();
            try { driver.Initialize(layout, placements, obstacleOutlines, arenaSideLength, player, run, view, registry); }
            catch { driver.Shutdown(); throw; }
            return driver;
        }

        private void Initialize(TrapLayoutDefinition layout, TrapPlacementSet placements,
            IReadOnlyList<IReadOnlyList<Vector2>> obstacleOutlines, float arenaSideLength, ITrapPlayerTarget player,
            RunController run, Func<Rect> view, ContentRegistry registry)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _square = CreateSquareSprite();
            var outlineShader = Resources.Load<Shader>("Shaders/SpriteDilatedOutline") ??
                throw new InvalidOperationException("Missing Resources/Shaders/SpriteDilatedOutline.");
            _outlineMaterial = new Material(outlineShader) { name = "Trap projectile outline", hideFlags = HideFlags.HideAndDontSave };
            _outlineMaterial.SetColor("_Color", ProjectileOutlineColor);
            _outlineMaterial.SetFloat("_WidthPixels", ProjectileOutlineWidthPixels);
            var deepest = 0f;
            foreach (var model in layout.Models.Values) deepest = Mathf.Max(deepest, model.DepthOffset);
            _projectileDepth = deepest > 0f ? -(deepest + ProjectileDepthMargin) : 0f;
            foreach (var pair in layout.Sprites)
                _sprites.Add(pair.Key, (registry ?? throw new ArgumentNullException(nameof(registry)))
                    .Get<SpriteDefinition>(new ContentId(pair.Value)).Sprite);
            _runtime = new TrapRuntime(layout, placements, player, obstacleOutlines, arenaSideLength);
            foreach (var trap in placements.Traps)
            {
                if (trap.ModelKey != null && _library == null)
                    _library = TrapPrefabLibrary.Load() ?? throw new InvalidOperationException(
                        $"Trap model '{trap.ModelKey}' needs the prefab library at Resources/{TrapPrefabLibrary.ResourcePath}.");
                _turrets.Add(CreateTurret(trap));
            }
            foreach (var barrel in placements.Barrels) _barrels.Add(CreateBarrel(barrel, layout.Barrels));
            Refresh(0f);
        }

        private static Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Trap placeholder square", filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private TurretView CreateTurret(TrapPlacement trap)
        {
            var type = trap.Type;
            var root = new GameObject($"Trap-{type.Id}");
            root.transform.SetParent(transform, false);
            root.transform.position = trap.Center;
            // A model-drawn turret's physical body follows the model's scale, so it never blocks beyond what is drawn.
            var scale = trap.ModelKey == null ? 1f : _layout.Models[trap.ModelKey].Scale;
            TrapObstacleFactory.Attach(root, type.BodyRadius * scale);
            var view = new TurretView { Placement = trap, Root = root.transform, HeadAngle = trap.RotationDegrees };
            view.Line = CreateRenderer("AimLine", root.transform, ProceduralShapeSprites.Beam, LineSortingOrder);
            view.Line.enabled = false;
            if (trap.ModelKey != null)
            {
                view.Ring = CreateRenderer("StateRing", root.transform, ProceduralShapeSprites.Ring, RingSortingOrder);
                view.Ring.transform.localScale = Vector3.one * (type.BodyRadius * scale * ModelRingFactor);
                return view;
            }
            view.Body = CreateRenderer("Body", root.transform, _square, BodySortingOrder);
            view.Body.transform.localScale = new Vector3(type.BodyRadius * 2f, type.BodyRadius * 2f, 1f);
            view.Head = CreateRenderer("Head", root.transform, _square, BodySortingOrder + 1);
            view.Head.transform.localScale = new Vector3(type.BodyRadius * 1.1f, type.BodyRadius * .4f, 1f);
            return view;
        }

        private void AttachModel(TurretView view)
        {
            var key = view.Placement.ModelKey;
            var prefab = _library.Find(key) ?? throw new InvalidOperationException($"Trap model '{key}' has no prefab in the library.");
            var definition = _layout.Models[key];
            var model = Instantiate(prefab, view.Root, false);
            model.name = $"TrapModel-{key}";
            // The model lies on its XZ ground with Y up; tilting it about X turns that ground to face the 2D camera, and the
            // forward offset keeps every fragment in front of the ground and prop sprites so it is never hidden behind them.
            model.transform.localPosition = new Vector3(0f, 0f, -definition.DepthOffset);
            model.transform.localRotation = Quaternion.Euler(-definition.TiltDegrees, 0f, 0f);
            model.transform.localScale = Vector3.one * definition.Scale;
            view.ModelHead = model.transform.Find(definition.HeadNode) ??
                throw new InvalidOperationException($"Trap model '{key}' has no head node '{definition.HeadNode}'.");
            var block = new MaterialPropertyBlock();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            {
                renderer.sortingOrder = BodySortingOrder;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                // A shader-expanded contour would not shrink with a smaller model; scale it (Blender shells have none).
                var material = renderer.sharedMaterial;
                if (material == null || !material.HasProperty(ExpansionProperty)) continue;
                var expansion = material.GetFloat(ExpansionProperty);
                if (Mathf.Approximately(expansion, 0f)) continue;
                renderer.GetPropertyBlock(block);
                block.SetFloat(ExpansionProperty, expansion * definition.Scale);
                renderer.SetPropertyBlock(block);
                block.Clear();
            }
            view.Model = model;
        }

        private BarrelView CreateBarrel(TrapBarrelPlacement barrel, TrapBarrelDefinition spec)
        {
            var root = new GameObject($"Barrel-{spec.Id}");
            root.transform.SetParent(transform, false);
            root.transform.position = barrel.Center;
            var view = new BarrelView { Placement = barrel };
            view.Contact = TrapObstacleFactory.Attach(root, spec.BodyRadius);
            // Plain and explosive barrels share this exact look until the fuse is lit.
            view.Body = CreateRenderer("Body", root.transform, _square, BodySortingOrder);
            if (_sprites.TryGetValue("barrel", out var sprite))
            {
                view.Body.sprite = sprite;
                view.Body.transform.localScale = Vector3.one * FitScale(sprite, spec.BodyRadius * 2f * BarrelSpriteFactor);
            }
            else
            {
                view.Body.transform.localScale = new Vector3(spec.BodyRadius * 2f, spec.BodyRadius * 2f, 1f);
                view.Body.color = BarrelColor;
            }
            view.Threat = CreateRenderer("Threat", root.transform, ProceduralShapeSprites.Disc, ThreatSortingOrder);
            view.Threat.transform.localScale = Vector3.one * (spec.BlastRadius * 2f);
            view.Threat.enabled = false;
            return view;
        }

        private static float FitScale(Sprite sprite, float worldSize) => worldSize / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);

        private static SpriteRenderer CreateRenderer(string name, Transform parent, Sprite sprite, int order)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void Update()
        {
            if (_runtime == null || _run.Model == null) return;
            var state = _run.Model.State;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped)
            {
                if (!_cleared) { _runtime.Clear(); _cleared = true; }
            }
            else if (state == RunState.Running)
            {
                _cleared = false;
                var view = _view();
                if (view.width > 0f) _runtime.Tick(Time.deltaTime, view.width);
            }
            Refresh(Time.deltaTime);
        }

        private void Refresh(float deltaTime)
        {
            var playerPosition = _player.Position;
            var radius = _runtime.RadiusWorld > 0f ? _runtime.RadiusWorld : _view().width * _layout.RadiusScreenWidths;
            foreach (var turret in _turrets) RefreshTurret(turret, playerPosition, radius, deltaTime);
            foreach (var barrel in _barrels) RefreshBarrel(barrel);
            RefreshProjectiles();
        }

        private void RefreshTurret(TurretView view, Vector2 player, float radius, float deltaTime)
        {
            var trap = view.Placement;
            // A far turret is switched off entirely (its body, ring and model): with a trap or two on every screen only the
            // ones around the player need drawing.
            var distanceSqr = (trap.Center - player).sqrMagnitude;
            var farSqr = radius * ModelHideRadii * radius * ModelHideRadii;
            var nearSqr = radius * ModelShowRadii * radius * ModelShowRadii;
            if (view.Root.gameObject.activeSelf ? distanceSqr > farSqr : distanceSqr > nearSqr)
            {
                if (view.Root.gameObject.activeSelf) view.Root.gameObject.SetActive(false);
                return;
            }
            if (!view.Root.gameObject.activeSelf) view.Root.gameObject.SetActive(true);
            var rage = trap.RageProgress;
            var wind = trap.TelegraphProgress;
            var heading = trap.HeadTargetDegrees(player);
            // Spinning heads follow the simulation exactly; the others turn quickly to their next pose (an aimed head tracks the player).
            view.HeadAngle = trap.Type.SpinDegreesPerSecond != 0f ? heading
                : Mathf.MoveTowardsAngle(view.HeadAngle, heading, HeadTurnDegreesPerSecond * deltaTime);
            var radians = view.HeadAngle * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            if (trap.ModelKey != null)
            {
                if (view.Model == null) AttachModel(view);
                if (view.ModelHead != null)
                    // Counter-clockwise world angles are clockwise turns about the model's up axis (left-handed rotation).
                    view.ModelHead.localRotation = Quaternion.Euler(0f, -view.HeadAngle, 0f);
                var ring = Color.Lerp(new Color(TurretColor.r, TurretColor.g, TurretColor.b, .3f), RageColor, rage);
                ring.a = Mathf.Min(1f, ring.a + .5f * wind);
                view.Ring.color = ring;
            }
            else
            {
                view.Body.color = Color.Lerp(TurretColor, RageColor, rage);
                view.Head.transform.localPosition = direction * (trap.Type.BodyRadius * .55f);
                view.Head.transform.localRotation = Quaternion.Euler(0f, 0f, view.HeadAngle);
                view.Head.color = Color.Lerp(TurretHeadColor, Color.white, wind);
            }
            var showLine = trap.State == TrapState.Telegraph && trap.Type.TelegraphLineLength > 0f;
            view.Line.enabled = showLine;
            if (!showLine) return;
            view.Line.transform.localPosition = direction * trap.Type.BodyRadius;
            view.Line.transform.localRotation = Quaternion.Euler(0f, 0f, view.HeadAngle);
            view.Line.transform.localScale = new Vector3(trap.Type.TelegraphLineLength, LineWidth, 1f);
            view.Line.color = new Color(1f, .3f, .2f, .15f + .6f * wind);
        }

        private void RefreshBarrel(BarrelView view)
        {
            var barrel = view.Placement;
            var spec = _runtime.Layout.Barrels;
            switch (barrel.State)
            {
                case TrapBarrelState.Intact:
                    view.Threat.enabled = false;
                    break;
                case TrapBarrelState.Fusing:
                    view.Threat.enabled = true;
                    view.Threat.color = new Color(1f, .2f, .1f, .12f + .4f * (1f - Mathf.Clamp01(barrel.FuseRemaining / spec.FuseSeconds)));
                    break;
                case TrapBarrelState.Exploded:
                    view.Body.enabled = false;
                    if (view.Contact != null) view.Contact.enabled = false;
                    var flash = 1f - Mathf.Clamp01(barrel.SecondsSinceExplosion / ExplosionFlashSeconds);
                    view.Threat.enabled = flash > 0f;
                    view.Threat.color = new Color(1f, .6f, .15f, .7f * flash);
                    break;
            }
        }

        private void RefreshProjectiles()
        {
            var live = _runtime.Projectiles;
            while (_projectiles.Count < live.Count)
            {
                var body = CreateRenderer($"Projectile-{_projectiles.Count}", transform, _square, ProjectileSortingOrder);
                var halo = CreateRenderer("Outline", body.transform, _square, ProjectileSortingOrder - 1);
                halo.sharedMaterial = _outlineMaterial;
                _projectiles.Add(body);
                _projectileHalos.Add(halo);
            }
            for (var i = 0; i < _projectiles.Count; i++)
            {
                var renderer = _projectiles[i];
                renderer.enabled = i < live.Count;
                _projectileHalos[i].enabled = renderer.enabled;
                if (!renderer.enabled) continue;
                var projectile = live[i];
                var definition = projectile.Definition;
                var diameter = definition.Radius * 2f;
                // Directional kinds (spear, bolt) point along their flight; round kinds turn in place.
                var directional = definition.Visual == "spear" || definition.Visual == "bolt";
                renderer.transform.position = new Vector3(projectile.Position.x, projectile.Position.y, _projectileDepth);
                renderer.transform.rotation = Quaternion.Euler(0f, 0f, directional ? projectile.FacingDegrees : projectile.SpinDegrees);
                if (_sprites.TryGetValue(definition.Visual, out var sprite))
                {
                    renderer.sprite = sprite;
                    renderer.color = Color.white;
                    var size = directional ? Mathf.Max(MinDirectionalLength, diameter * DirectionalLengthFactor) : diameter * RoundSpriteFactor;
                    renderer.transform.localScale = Vector3.one * FitScale(sprite, size);
                }
                else
                {
                    renderer.sprite = _square;
                    renderer.color = ProjectileColor;
                    renderer.transform.localScale = directional
                        ? new Vector3(Mathf.Max(MinDirectionalLength, diameter * DirectionalLengthFactor), diameter, 1f)
                        : new Vector3(diameter, diameter, 1f);
                }
                // The outline copies the projectile's sprite; the shader dilates it, so no resizing (placeholders just grow a little).
                _projectileHalos[i].sharedMaterial = OutlineMaterialFor(renderer.sprite);
                _projectileHalos[i].sprite = renderer.sprite;
                _projectileHalos[i].transform.localScale = renderer.sprite == _square ? Vector3.one * PlaceholderOutlineScale : Vector3.one;
            }
        }

        private Material OutlineMaterialFor(Sprite sprite)
        {
            if (_outlineMaterials.TryGetValue(sprite, out var material)) return material;
            material = new Material(_outlineMaterial) { name = $"Trap projectile outline {sprite.name}", hideFlags = HideFlags.HideAndDontSave };
            material.SetTexture("_MainTex", sprite.texture);
            _outlineMaterials.Add(sprite, material);
            return material;
        }

        /// <summary>Removes the traps and the scene objects; safe to call more than once.</summary>
        public void Shutdown()
        {
            _runtime?.Clear();
            _runtime = null;
            _turrets.Clear();
            _barrels.Clear();
            _projectiles.Clear();
            _projectileHalos.Clear();
            _sprites.Clear();
            foreach (var material in _outlineMaterials.Values)
                if (material != null) { if (Application.isPlaying) Destroy(material); else DestroyImmediate(material); }
            _outlineMaterials.Clear();
            if (_outlineMaterial != null)
            {
                if (Application.isPlaying) Destroy(_outlineMaterial); else DestroyImmediate(_outlineMaterial);
                _outlineMaterial = null;
            }
            if (_square != null)
            {
                var texture = _square.texture;
                if (Application.isPlaying) { Destroy(_square); Destroy(texture); } else { DestroyImmediate(_square); DestroyImmediate(texture); }
                _square = null;
            }
            if (this == null) return;
            if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
        }

        private void OnDestroy() { _runtime = null; }
    }
}
