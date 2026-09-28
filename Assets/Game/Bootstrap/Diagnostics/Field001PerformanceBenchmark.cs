#if UNITY_EDITOR || FIELD001_PERFORMANCE_BENCHMARK
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Meta;
using Game.Pooling;
using Game.Progression;
using Game.Run;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Game.Bootstrap
{
    /// <summary>
    /// Development-player-only FIELD-001 performance acceptance runner. Launch a development build with
    /// <c>--field001-performance --benchmark-output=&lt;path&gt;</c>; it uses an isolated memory profile,
    /// records real frames at 1x, writes one JSON report, and quits without touching the player's save.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class Field001PerformanceBenchmark : MonoBehaviour
    {
        private const int TargetFrameRate = 60;
        private const float WindowSeconds = 60f;
        private const float WarmupSeconds = 30f;
        private const int StressOrdinaryCount = 250;
        private const int RestartCount = 10;
        private const double P95BudgetMilliseconds = 16.7;
        private const double P99BudgetMilliseconds = 33.3;
        private const double StallBudgetMilliseconds = 100;
        private const long ManagedGrowthBudgetBytes = 5L * 1024L * 1024L;

        private readonly List<EnemyRuntime> _extraEnemies = new List<EnemyRuntime>();
        private readonly Dictionary<ContentId, int> _targetLevels = new Dictionary<ContentId, int>();
        private readonly List<JObject> _windows = new List<JObject>();
        private readonly List<JObject> _restarts = new List<JObject>();
        private readonly List<string> _errors = new List<string>();
        private readonly List<float> _frameMilliseconds = new List<float>(4096);
        private readonly List<double> _cpuMilliseconds = new List<double>(4096);
        private readonly List<double> _gpuMilliseconds = new List<double>(4096);
        private readonly FrameTiming[] _timing = new FrameTiming[1];

        private GameObjectPool<EnemyRuntime> _extraEnemyPool;
        private GameObjectPool<EnemyProjectileRuntime> _extraProjectilePool;
        private GameplayCompositionRoot _root;
        private RunController _run;
        private PlayerCharacterRuntime _player;
        private LevelUpDraftRuntime _draft;
        private PlayerExperienceRuntime _experience;
        private ContinuousFixtureEnemySpawner _spawner;
        private ProfilerRecorder _gcAllocRecorder;
        private string _outputPath;
        private string _profileOutputPath;
        private float _profileSeconds = 15f;
        private string _commit;
        private bool _dirty;
        private double _sceneLoadedAt;
        private double _runStartedAt;
        private long _gcAllocEvents;
        private int _bookSequence;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var args = Environment.GetCommandLineArgs();
            if (!args.Any(value => string.Equals(value, "--field001-performance", StringComparison.Ordinal)))
                return;
            var host = new GameObject(nameof(Field001PerformanceBenchmark));
            DontDestroyOnLoad(host);
            host.AddComponent<Field001PerformanceBenchmark>();
        }

        private void Awake()
        {
            _outputPath = Argument("--benchmark-output=") ??
                Path.Combine(Application.persistentDataPath, "field001-performance.json");
            _commit = Argument("--benchmark-commit=") ?? "unknown";
            _dirty = string.Equals(Argument("--benchmark-dirty="), "true", StringComparison.OrdinalIgnoreCase);
            _profileOutputPath = Argument("--benchmark-profile-output=");
            var profileSeconds = Argument("--benchmark-profile-seconds=");
            if (profileSeconds != null && (!float.TryParse(profileSeconds, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out _profileSeconds) || _profileSeconds <= 0f))
                throw new ArgumentException("--benchmark-profile-seconds must be a positive invariant number.");
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            SceneManager.sceneLoaded += ConfigureScene;
        }

        private void Start() => StartCoroutine(Run());

        private void ConfigureScene(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Gameplay") return;
            _sceneLoadedAt = Time.realtimeSinceStartupAsDouble;
            _root = FindAnyObjectByType<GameplayCompositionRoot>();
            if (_root == null || _root.Profile != null) return;
            _root.ConfigureProfile(new ProfileService(MetaCatalog.Load(), new MemoryProfileStore()));
        }

        private IEnumerator Run()
        {
            var startedAt = DateTime.UtcNow;
            var stack = new Stack<IEnumerator>();
            stack.Push(RunCore());
            while (stack.Count > 0)
            {
                object current;
                try
                {
                    if (!stack.Peek().MoveNext())
                    {
                        (stack.Pop() as IDisposable)?.Dispose();
                        continue;
                    }
                    current = stack.Peek().Current;
                }
                catch (Exception exception)
                {
                    _errors.Add(exception.ToString());
                    break;
                }
                if (current is IEnumerator nested) stack.Push(nested);
                else yield return current;
            }
            while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
            StopProfilerCapture();
            if (_gcAllocRecorder.Valid) _gcAllocRecorder.Dispose();
            CleanupExtraEnemies();
            WriteReport(startedAt);
            SceneManager.sceneLoaded -= ConfigureScene;
            yield return null;
            var completed = _profileOutputPath != null
                ? _errors.Count == 0 && _windows.Count == 1 && File.Exists(_profileOutputPath)
                : _errors.Count == 0 && WindowsPass() && RestartsPass();
            Application.Quit(completed ? 0 : 2);
        }

        private IEnumerator RunCore()
        {
            yield return WaitUntil(() => _root != null && _root.CanPlay && _root.AtMainMenu, 30, "production menu initialization");
            StartProductionRun();
            _runStartedAt = Time.realtimeSinceStartupAsDouble;
            BindRuntime();
            ConfigureTargetBuild();
            yield return CompleteTargetBuild();
            yield return Warmup(WarmupSeconds);

            if (_profileOutputPath != null)
            {
                yield return AdvanceTo(780f);
                PrepareStress();
                yield return Warmup(WarmupSeconds);
                StartProfilerCapture();
                yield return CaptureWindow("final-stress-profile", _profileSeconds, stress: true);
                StopProfilerCapture();
                yield break;
            }

            yield return AdvanceTo(270f);
            yield return CaptureWindow("minute-5", WindowSeconds, stress: false);
            yield return AdvanceTo(570f);
            yield return CaptureWindow("minute-10", WindowSeconds, stress: false);
            yield return AdvanceTo(780f);
            PrepareStress();
            yield return Warmup(WarmupSeconds);
            yield return CaptureWindow("final-stress", WindowSeconds, stress: true);

            CleanupExtraEnemies();
            yield return RestartAudit();
        }

        private void StartProductionRun()
        {
            _root.UseReferenceSeeds = true;
            _root.Play();
            var character = _root.Catalog.RunSetup.StartingCharacterId;
            if (!_root.TryStartCharacter(character)) throw new InvalidOperationException("Could not select the starting character.");
            if (!_root.TryStartField(_root.Catalog.Fields.DefaultFieldId)) throw new InvalidOperationException("Could not select FIELD-001.");
        }

        private void BindRuntime()
        {
            _run = FindAnyObjectByType<RunController>();
            _player = FindAnyObjectByType<PlayerCharacterRuntime>();
            _draft = FindAnyObjectByType<LevelUpDraftRuntime>();
            _experience = FindAnyObjectByType<PlayerExperienceRuntime>();
            _spawner = FindAnyObjectByType<ContinuousFixtureEnemySpawner>();
            if (_run?.Model == null || _player?.Health == null || _draft == null || _experience == null || _spawner == null)
                throw new InvalidOperationException("Production runtime did not compose every benchmark dependency.");
            _player.Health.IsLocked = true;
        }

        private void ConfigureTargetBuild()
        {
            _draft.AddDevelopmentDraftEntries(_root.Catalog.BuildEntries);
            _draft.GrantDevelopmentRerolls(10000);
            var sets = new[] { "SET-001", "SET-004", "SET-010", "SET-017" }
                .Select(id => _root.Catalog.Sets.Single(set => set.Id == new ContentId(id))).ToArray();
            foreach (var set in sets)
            {
                _targetLevels[set.Id] = 1;
                foreach (var component in set.Recipe)
                    _targetLevels[component.Id] = Math.Max(_targetLevels.TryGetValue(component.Id, out var level) ? level : 0,
                        component.MinimumLevel);
            }
            var activeIds = new HashSet<ContentId>(_targetLevels.Where(pair =>
                _root.Catalog.ActiveSkills.Any(skill => skill.Id == pair.Key)).Select(pair => pair.Key));
            foreach (var skill in _root.Catalog.ActiveSkills)
                if (activeIds.Count < PlayerBuild.ActiveSlotCapacity) activeIds.Add(skill.Id);
            foreach (var id in activeIds) _targetLevels[id] = BuildEntryDefinition.MaxLevel;
        }

        private IEnumerator CompleteTargetBuild()
        {
            var deadline = Time.realtimeSinceStartupAsDouble + 30;
            while (!TargetBuildComplete())
            {
                if (Time.realtimeSinceStartupAsDouble > deadline)
                    throw new TimeoutException("The six-skill/four-set benchmark build could not be assembled.");
                if (!_draft.IsDraftOpen)
                {
                    _draft.RequestBook(Guid.NewGuid(), _run.Model.RunId, new ContentId($"BENCHMARK-BOOK-{++_bookSequence:000}"));
                    yield return null;
                    continue;
                }
                var selected = false;
                for (var reroll = 0; reroll < 200 && _draft.IsDraftOpen; reroll++)
                {
                    var option = _draft.CurrentDraft.Options
                        .Where(candidate => Needs(candidate.Definition.Id, candidate.ResultingLevel))
                        .OrderBy(candidate => candidate.Definition.Kind == BuildEntryKind.Set ? 1 : 0)
                        .FirstOrDefault();
                    if (option.Definition != null)
                    {
                        selected = _draft.Select(option.Definition.Id);
                        break;
                    }
                    _draft.Reroll();
                }
                if (!selected && _draft.IsDraftOpen) yield return null;
            }
        }

        private bool Needs(ContentId id, int resultingLevel) =>
            _targetLevels.TryGetValue(id, out var target) && resultingLevel <= target;

        private bool TargetBuildComplete()
        {
            foreach (var pair in _targetLevels)
                if (!_draft.Build.TryGetEntry(pair.Key, out var entry) || entry.Level < pair.Value) return false;
            return _draft.Build.ActiveCount == PlayerBuild.ActiveSlotCapacity && _draft.Build.SetCount >= 4;
        }

        private IEnumerator AdvanceTo(float elapsed)
        {
            if (!_run.SetSpeed(5)) throw new InvalidOperationException("Could not enable benchmark fast-forward.");
            while (_run.Model.Elapsed < elapsed)
            {
                AutoResolveDraft();
                MovePlayer();
                LockEncounterHealth();
                yield return null;
            }
            if (!_run.SetSpeed(1)) throw new InvalidOperationException("Could not restore real-time speed.");
        }

        private IEnumerator Warmup(float seconds)
        {
            var end = Time.realtimeSinceStartupAsDouble + seconds;
            while (Time.realtimeSinceStartupAsDouble < end)
            {
                AutoResolveDraft();
                MovePlayer();
                LockEncounterHealth();
                yield return null;
            }
        }

        private IEnumerator CaptureWindow(string name, float seconds, bool stress)
        {
            _frameMilliseconds.Clear();
            _cpuMilliseconds.Clear();
            _gpuMilliseconds.Clear();
            _gcAllocEvents = 0;
            _gcAllocRecorder = new ProfilerRecorder(ProfilerCategory.Memory, "GC.Alloc", 1,
                ProfilerRecorderOptions.WrapAroundWhenCapacityReached |
                ProfilerRecorderOptions.SumAllSamplesInFrame);
            var end = Time.realtimeSinceStartupAsDouble + seconds;
            while (Time.realtimeSinceStartupAsDouble < end)
            {
                AutoResolveDraft();
                MovePlayer();
                if (stress) MaintainStress();
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
                _frameMilliseconds.Add(Time.unscaledDeltaTime * 1000f);
                if (FrameTimingManager.GetLatestTimings(1, _timing) > 0)
                {
                    _cpuMilliseconds.Add(_timing[0].cpuFrameTime);
                    if (_timing[0].gpuFrameTime > 0) _gpuMilliseconds.Add(_timing[0].gpuFrameTime);
                }
                if (_gcAllocRecorder.Valid && _gcAllocRecorder.Count > 0) _gcAllocEvents += _gcAllocRecorder.LastValue;
            }
            _gcAllocRecorder.Dispose();
            _windows.Add(Window(name, stress, seconds));
        }

        private JObject Window(string name, bool stress, float seconds)
        {
            var sorted = _frameMilliseconds.Select(value => (double)value).OrderBy(value => value).ToArray();
            var p95 = Percentile(sorted, 0.95);
            var p99 = Percentile(sorted, 0.99);
            var max = sorted.Length == 0 ? 0 : sorted[sorted.Length - 1];
            return new JObject
            {
                ["name"] = name,
                ["stress"] = stress,
                ["samples"] = sorted.Length,
                ["elapsedStartSeconds"] = _run.Model.Elapsed - seconds,
                ["elapsedEndSeconds"] = _run.Model.Elapsed,
                ["ordinaryAlive"] = _spawner.AliveCount + _extraEnemies.Count(enemy => enemy != null && enemy.IsAlive),
                ["bossAlive"] = FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None).Count(enemy => enemy.Category == EnemyCategory.Boss && enemy.IsAlive),
                ["travelerAlive"] = _root.Travelers?.Snapshot.Count ?? 0,
                ["activeSkills"] = FindAnyObjectByType<Game.ActiveSkill.PlayerActiveSkillSetRuntime>()?.SkillCount ?? 0,
                ["sets"] = _draft.Build.SetCount,
                ["frameP95Ms"] = p95,
                ["frameP99Ms"] = p99,
                ["frameMaxMs"] = max,
                ["cpuP95Ms"] = Percentile(_cpuMilliseconds.OrderBy(value => value).ToArray(), 0.95),
                ["gpuP95Ms"] = Percentile(_gpuMilliseconds.OrderBy(value => value).ToArray(), 0.95),
                ["gcAllocationEvents"] = _gcAllocEvents,
                ["pass"] = sorted.Length > 0 && p95 <= P95BudgetMilliseconds && p99 <= P99BudgetMilliseconds && max <= StallBudgetMilliseconds
            };
        }

        private void PrepareStress()
        {
            _spawner.Tick(_run.Model.Elapsed, 1000f, true);
            _extraEnemyPool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _extraProjectilePool ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
            MaintainStress();
            var definitions = _root.Catalog.Travelers.Definitions.Values.Take(3).ToArray();
            for (var i = _root.Travelers?.Snapshot.Count ?? 0; i < 3 && i < definitions.Length; i++)
                _root.Travelers.Spawn(definitions[i].Id, _run.Model.Elapsed + WarmupSeconds + WindowSeconds, 1f);
        }

        private void StartProfilerCapture()
        {
            var directory = Path.GetDirectoryName(_profileOutputPath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            if (File.Exists(_profileOutputPath)) File.Delete(_profileOutputPath);
            Profiler.logFile = _profileOutputPath;
            Profiler.enableBinaryLog = true;
            Profiler.enabled = true;
        }

        private void StopProfilerCapture()
        {
            if (!Profiler.enabled && !Profiler.enableBinaryLog) return;
            Profiler.enabled = false;
            Profiler.enableBinaryLog = false;
            Profiler.logFile = string.Empty;
        }

        private void MaintainStress()
        {
            _spawner.Tick(_run.Model.Elapsed, 1000f, true);
            _extraEnemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);
            var required = StressOrdinaryCount - _spawner.AliveCount - _extraEnemies.Count;
            var definitions = _root.FieldConfiguration.Enemies;
            for (var i = 0; i < required; i++)
            {
                var definition = definitions[(_extraEnemies.Count + i) % definitions.Count];
                var body = EnemyBodyVisual.Resolve(definition, _root.Catalog.Registry);
                var angle = (_extraEnemies.Count + i) * Mathf.PI * 2f / Math.Max(1, required);
                var enemy = EnemyFactory.Spawn(definition,
                    (Vector2)_player.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 10f,
                    _player.transform, _run, transform, body.Sprite, _extraEnemyPool, _extraProjectilePool,
                    motionProfile: body.Motion, contact: body.Contact,
                    deathPresentation: _root.Catalog.EnemyDeathPresentation,
                    groundShadowPresentation: _root.Catalog.GroundShadowPresentation,
                    contentRegistry: _root.Catalog.Registry);
                enemy.Health.IsLocked = true;
                _extraEnemies.Add(enemy);
            }
        }

        private void LockEncounterHealth()
        {
            if (_player?.Health != null) _player.Health.IsLocked = true;
            foreach (var enemy in FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None))
                if (enemy.Category != EnemyCategory.Ordinary && enemy.Health != null) enemy.Health.IsLocked = true;
        }

        private void MovePlayer()
        {
            if (_player == null) return;
            var angle = (float)(Time.realtimeSinceStartupAsDouble * 0.35);
            _player.transform.position = new Vector3(Mathf.Cos(angle) * 12f, Mathf.Sin(angle) * 12f, _player.transform.position.z);
        }

        private void AutoResolveDraft()
        {
            if (_draft != null && _draft.IsDraftOpen) _draft.Select(_draft.CurrentDraft.Options[0].Definition.Id);
        }

        private IEnumerator RestartAudit()
        {
            yield return RestartCycle(-1);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            yield return null;
            var firstManaged = 0L;
            for (var cycle = 0; cycle < RestartCount; cycle++)
            {
                yield return RestartCycle(cycle);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                yield return null;
                var managed = GC.GetTotalMemory(false);
                if (cycle == 0) firstManaged = managed;
                var allEnemies = FindObjectsByType<EnemyRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                var allProjectiles = FindObjectsByType<EnemyProjectileRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                _restarts.Add(new JObject
                {
                    ["cycle"] = cycle + 1,
                    ["enemyCapacity"] = allEnemies.Length,
                    ["enemyLeased"] = allEnemies.Count(enemy => enemy.gameObject.activeInHierarchy),
                    ["projectileCapacity"] = allProjectiles.Length,
                    ["projectileLeased"] = allProjectiles.Count(projectile => projectile.gameObject.activeInHierarchy),
                    ["enemyRegistryCount"] = EnemyRegistry.Count,
                    ["managedBytesAfterGc"] = managed,
                    ["managedGrowthFromFirstBytes"] = managed - firstManaged
                });
            }
        }

        private IEnumerator RestartCycle(int cycle)
        {
            if (_root.IsInitialized)
            {
                _root.QuitProfileRun();
                yield return WaitUntil(() => _root.ProfileSaveTask.IsCompleted, 10, "profile save before restart");
                _root.MainMenu();
                yield return null;
            }
            StartProductionRun();
            BindRuntime();
            _spawner.Tick(_run.Model.Elapsed, 100f, true);
            yield return null;
            _root.QuitProfileRun();
            yield return WaitUntil(() => _root.ProfileSaveTask.IsCompleted, 10, "profile save after restart");
            _root.MainMenu();
            yield return null;
        }

        private void CleanupExtraEnemies()
        {
            foreach (var enemy in _extraEnemies.ToArray()) if (enemy != null && enemy.IsAlive) enemy.Despawn();
            _extraEnemies.Clear();
        }

        private void WriteReport(DateTime startedAt)
        {
            var directory = Path.GetDirectoryName(_outputPath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            var report = new JObject
            {
                ["schemaVersion"] = 1,
                ["startedUtc"] = startedAt.ToString("O"),
                ["finishedUtc"] = DateTime.UtcNow.ToString("O"),
                ["commit"] = _commit,
                ["dirty"] = _dirty,
                ["unityVersion"] = Application.unityVersion,
                ["buildType"] = Debug.isDebugBuild ? "Development" : "Release",
                ["platform"] = Application.platform.ToString(),
                ["resolution"] = $"{Screen.width}x{Screen.height}",
                ["refreshRate"] = Screen.currentResolution.refreshRateRatio.value,
                ["quality"] = QualitySettings.names[QualitySettings.GetQualityLevel()],
                ["renderScale"] = RenderScale(),
                ["warningStackTraces"] = false,
                ["profileOutput"] = _profileOutputPath,
                ["cpu"] = SystemInfo.processorType,
                ["cpuCores"] = SystemInfo.processorCount,
                ["gpu"] = SystemInfo.graphicsDeviceName,
                ["graphicsApi"] = SystemInfo.graphicsDeviceType.ToString(),
                ["systemMemoryMB"] = SystemInfo.systemMemorySize,
                ["graphicsMemoryMB"] = SystemInfo.graphicsMemorySize,
                ["sceneAndRunLoadMs"] = (_runStartedAt - _sceneLoadedAt) * 1000d,
                ["budgets"] = new JObject
                {
                    ["frameP95Ms"] = P95BudgetMilliseconds,
                    ["frameP99Ms"] = P99BudgetMilliseconds,
                    ["stallMs"] = StallBudgetMilliseconds,
                    ["managedRestartGrowthBytes"] = ManagedGrowthBudgetBytes,
                    ["stressOrdinary"] = StressOrdinaryCount,
                    ["restartCount"] = RestartCount
                },
                ["windows"] = JArray.FromObject(_windows),
                ["restarts"] = JArray.FromObject(_restarts),
                ["errors"] = JArray.FromObject(_errors),
                ["frameWindowsPass"] = WindowsPass(),
                ["restartAuditPass"] = RestartsPass(),
                ["pass"] = _errors.Count == 0 && WindowsPass() && RestartsPass()
            };
            File.WriteAllText(_outputPath, report.ToString(Formatting.Indented));
            Debug.Log($"FIELD-001 performance report: {_outputPath}");
        }

        private bool WindowsPass() => _windows.Count == 3 && _windows.All(window => (bool)window["pass"]);

        private bool RestartsPass()
        {
            if (_restarts.Count != RestartCount) return false;
            if (_restarts.Any(row => (int)row["enemyLeased"] != 0 || (int)row["projectileLeased"] != 0 ||
                                     (int)row["enemyRegistryCount"] != 0)) return false;
            var first = (long)_restarts[0]["managedBytesAfterGc"];
            var last = (long)_restarts[_restarts.Count - 1]["managedBytesAfterGc"];
            var tailEnemy = _restarts.Skip(RestartCount - 3).Select(row => (int)row["enemyCapacity"]).Distinct().Count();
            var tailProjectile = _restarts.Skip(RestartCount - 3).Select(row => (int)row["projectileCapacity"]).Distinct().Count();
            return last - first <= ManagedGrowthBudgetBytes && tailEnemy == 1 && tailProjectile == 1;
        }

        private static double Percentile(IReadOnlyList<double> sorted, double percentile)
        {
            if (sorted == null || sorted.Count == 0) return 0;
            var index = Math.Min(sorted.Count - 1, Math.Max(0, (int)Math.Ceiling(sorted.Count * percentile) - 1));
            return sorted[index];
        }

        private static IEnumerator WaitUntil(Func<bool> condition, double timeoutSeconds, string operation)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + timeoutSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartupAsDouble > deadline) throw new TimeoutException(operation + " timed out.");
                yield return null;
            }
        }

        private static string Argument(string prefix)
        {
            foreach (var value in Environment.GetCommandLineArgs())
                if (value.StartsWith(prefix, StringComparison.Ordinal)) return value.Substring(prefix.Length);
            return null;
        }

        private static float RenderScale()
        {
            var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            var property = pipeline?.GetType().GetProperty("renderScale");
            return property?.GetValue(pipeline) is float value ? value : 1f;
        }
    }
}
#endif
