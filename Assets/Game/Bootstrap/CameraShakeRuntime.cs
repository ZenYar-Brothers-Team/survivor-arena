using Game.Combat;
using Game.Run;
using Game.Settings;
using Game.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
namespace Game.Bootstrap
{
    /// <summary>Offset exists only inside camera rendering; gameplay observes the stable follow camera.</summary>
    public sealed class CameraShakeRuntime : MonoBehaviour
    {
        private Camera _camera;
        private Health _health;
        private RunModel _run;
        private ISettingsService _settings;
        private SettingsConfig _config;
        private ScreenShakeRequestGate _gate;
        private float _elapsed;
        private bool _active, _rendering;
        private Vector3 _baseline;
        public Vector2 Offset { get; private set; }
        public void Initialize(Camera camera, Health health, RunModel run, ISettingsService settings, SettingsConfig config)
        {
            Shutdown(); _camera=camera; _health=health; _run=run; _settings=settings; _config=config;
            _gate=new ScreenShakeRequestGate(settings); _gate.Requested+=Requested;
            health.Damaged+=Damage; run.StateChanged+=State; settings.Changed+=Preference;
            RenderPipelineManager.beginCameraRendering+=Begin; RenderPipelineManager.endCameraRendering+=End;
        }
        private void Damage(float actual)
        {
            if (!isActiveAndEnabled || actual <= 0 || _health.IsDead || _camera == null) return;
            _gate.Request(_run.State == RunState.Running, _config.ShakeFraction * 2 * _camera.orthographicSize, _config.ShakeSeconds);
        }
        private void Requested(float amplitude, float seconds) { _elapsed=0; _active=true; }
        private void State(RunState state) { if(state!=RunState.Running)Clear(); }
        private void Preference() { if(!_settings.ScreenShakeEnabled)Clear(); }
        private void LateUpdate()
        {
            if(!_active||_camera==null)return;
            _elapsed+=Time.deltaTime;
            if(_elapsed>=_config.ShakeSeconds) { Clear(); return; }
            var amplitude=_config.ShakeFraction*2*_camera.orthographicSize*(1-_elapsed/_config.ShakeSeconds);
            var phase=2*Mathf.PI*_config.ShakeFrequency*_elapsed;
            Offset=new Vector2(Mathf.Sin(phase),Mathf.Cos(phase))*amplitude;
        }
        private void Begin(ScriptableRenderContext context, Camera camera)
        {
            if(camera!=_camera||!_active||_rendering)return;
            _baseline=camera.transform.position; _rendering=true;
            camera.transform.position=_baseline+(Vector3)Offset;
        }
        private void End(ScriptableRenderContext context, Camera camera) { if(camera==_camera)Restore(); }
        private void Restore() { if(_rendering&&_camera!=null)_camera.transform.position=_baseline; _rendering=false; }
        private void Clear() { Restore(); _active=false; _elapsed=0; Offset=Vector2.zero; }
        public void Shutdown()
        {
            Clear();
            if (_gate != null) _gate.Requested-=Requested; _gate=null;
            if(_health!=null)_health.Damaged-=Damage;
            if(_run!=null)_run.StateChanged-=State;
            if(_settings!=null)_settings.Changed-=Preference;
            RenderPipelineManager.beginCameraRendering-=Begin; RenderPipelineManager.endCameraRendering-=End;
            _health=null; _run=null; _settings=null; _camera=null;
        }
        private void OnDisable() => Clear();
        private void OnDestroy() => Shutdown();
    }
}
