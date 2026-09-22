using System;
using Game.Combat;
using Game.Run;
using UnityEngine;

namespace Game.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpritePresentationRig))]
    public sealed class SpritePresentationRuntime : MonoBehaviour
    {
        [SerializeField]
        private SpritePresentationRig rig;

        private ProceduralSpriteAnimator _animator;
        private SpriteMotionProfile _profile;
        private Health _health;
        private Rigidbody2D _motionBody;
        private RunController _runController;
        private Vector3 _baselinePosition;
        private Vector3 _authoredPosition;
        private Quaternion _baselineRotation;
        private Vector3 _baselineScale;
        private Color _baselineColor;
        private bool _baselineFlipX;
        private Sprite _baselineSprite;
        private bool _baselineEnabled;
        private SpritePresentationPreviewMotion _previewMotion;
        private bool _initialized;

        public bool IsInitialized => _initialized;
        public SpritePresentationPreviewMotion PreviewMotion => _previewMotion;

        private void Awake()
        {
            if (rig == null)
                rig = GetComponent<SpritePresentationRig>();
        }

        private void Update()
        {
            if (_initialized)
                Tick(Time.deltaTime);
        }

        public void Initialize(
            SpriteDefinition sprite,
            SpriteMotionProfile profile,
            Health health,
            Rigidbody2D motionBody,
            RunController runController)
        {
            if (_initialized)
                Shutdown();

            if (sprite == null)
                throw new ArgumentNullException(nameof(sprite));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _motionBody = motionBody != null ? motionBody : throw new ArgumentNullException(nameof(motionBody));
            _runController = runController != null ? runController : throw new ArgumentNullException(nameof(runController));
            if (_runController.Model == null)
                throw new InvalidOperationException("Sprite presentation requires an initialized run model.");

            if (rig == null)
                rig = GetComponent<SpritePresentationRig>();
            rig.Validate();

            _authoredPosition = rig.BodyRoot.localPosition;
            _baselinePosition = _authoredPosition - Vector3.up * (sprite.Contact?.CenterY ?? 0);
            _baselineRotation = rig.BodyRoot.localRotation;
            _baselineScale = rig.BodyRoot.localScale;
            _baselineColor = rig.BodyRenderer.color;
            _baselineFlipX = rig.BodyRenderer.flipX;
            _baselineSprite = rig.BodyRenderer.sprite;
            _baselineEnabled = rig.BodyRenderer.enabled;

            rig.BodyRenderer.sprite = sprite.Sprite;
            rig.BodyRenderer.enabled = true;
            _animator = new ProceduralSpriteAnimator(profile);
            _previewMotion = SpritePresentationPreviewMotion.Live;
            _health.Damaged += HandleDamaged;
            _initialized = true;
            ApplyPose(_animator.CurrentPose);
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized)
                throw new InvalidOperationException("Sprite presentation runtime must be initialized before ticking.");

            var isRunning = _runController.Model.State == RunState.Running;
            ApplyPose(_animator.Tick(deltaTime, isRunning, ResolveVelocity()));
        }

        public void SetPreviewMotion(SpritePresentationPreviewMotion previewMotion)
        {
            if (!_initialized)
                throw new InvalidOperationException("Sprite presentation runtime must be initialized before previewing motion.");
            if (!Enum.IsDefined(typeof(SpritePresentationPreviewMotion), previewMotion))
                throw new ArgumentOutOfRangeException(nameof(previewMotion));

            _previewMotion = previewMotion;
            Tick(0f);
        }

        public void ResetPresentation()
        {
            if (!_initialized)
                throw new InvalidOperationException("Sprite presentation runtime must be initialized before reset.");

            _animator.Reset();
            _previewMotion = SpritePresentationPreviewMotion.Live;
            ApplyPose(_animator.CurrentPose);
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            _health.Damaged -= HandleDamaged;
            RestoreBaseline();
            _animator = null;
            _profile = null;
            _health = null;
            _motionBody = null;
            _runController = null;
            _previewMotion = SpritePresentationPreviewMotion.Live;
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void HandleDamaged(float appliedDamage)
        {
            _animator.PlayHit(appliedDamage);
            ApplyPose(_animator.CurrentPose);
        }

        private Vector2 ResolveVelocity()
        {
            return _previewMotion switch
            {
                SpritePresentationPreviewMotion.Idle => Vector2.zero,
                SpritePresentationPreviewMotion.Left => Vector2.left * _profile.ReferenceSpeed,
                SpritePresentationPreviewMotion.Right => Vector2.right * _profile.ReferenceSpeed,
                _ => _motionBody.linearVelocity
            };
        }

        private void ApplyPose(SpritePose pose)
        {
            rig.BodyRoot.localPosition = _baselinePosition + pose.PositionOffset;
            rig.BodyRoot.localRotation = _baselineRotation * Quaternion.Euler(0f, 0f, pose.RotationDegrees);
            rig.BodyRoot.localScale = Vector3.Scale(
                _baselineScale,
                new Vector3(pose.ScaleMultiplier.x, pose.ScaleMultiplier.y, 1f));
            rig.BodyRenderer.flipX = pose.FlipX;
            rig.BodyRenderer.color = Color.Lerp(_baselineColor, _profile.HitFlashColor, pose.FlashAmount);
        }

        private void RestoreBaseline()
        {
            rig.BodyRoot.localPosition = _authoredPosition;
            rig.BodyRoot.localRotation = _baselineRotation;
            rig.BodyRoot.localScale = _baselineScale;
            rig.BodyRenderer.color = _baselineColor;
            rig.BodyRenderer.flipX = _baselineFlipX;
            rig.BodyRenderer.sprite = _baselineSprite;
            rig.BodyRenderer.enabled = _baselineEnabled;
        }
    }
}
