using System;
using Game.Content;
using Game.Presentation;
using Game.Run;
using Game.Zones;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Bootstrap
{
    /// <summary>
    /// Platform fields (FIELD-009): while the player stands outside every platform and bridge, they take the void's
    /// damage per second. Ticks only while the run is Running, so pause advances nothing.
    /// </summary>
    public sealed class FieldVoidDamageDriver : MonoBehaviour
    {
        private FieldPlatformLayout _layout;
        private IZonePlayerTarget _player;
        private RunController _run;
        private ContentId _source;

        public FieldPlatformLayout Layout => _layout;

        public static FieldVoidDamageDriver Create(FieldPlatformLayout layout, IZonePlayerTarget player, RunController run,
            ContentId source, Scene scene)
        {
            var root = new GameObject("FieldVoidDamage");
            SceneManager.MoveGameObjectToScene(root, scene);
            var driver = root.AddComponent<FieldVoidDamageDriver>();
            driver._layout = layout ?? throw new ArgumentNullException(nameof(layout));
            driver._player = player ?? throw new ArgumentNullException(nameof(player));
            driver._run = run ?? throw new ArgumentNullException(nameof(run));
            driver._source = source;
            return driver;
        }

        private void Update()
        {
            if (_layout == null || _run.Model == null || _run.Model.State != RunState.Running || !_player.IsAlive) return;
            if (!_layout.IsWalkable(_player.Position)) _player.Damage(_layout.Profile.VoidDamagePerSecond * Time.deltaTime, _source);
        }

        public void Shutdown()
        {
            _layout = null;
            if (this != null) Destroy(gameObject);
        }
    }
}
