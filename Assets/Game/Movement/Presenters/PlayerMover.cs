using Game.Run;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference moveAction;

        [SerializeField]
        private RunController runController;

        [SerializeField]
        private Transform spawnPoint;

        public string MovementBindings => moveAction != null ? string.Join(", ", moveAction.action.bindings.Where(b => !b.isComposite).Select(b => InputControlPath.ToHumanReadableString(b.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice)).Distinct()) : "Unavailable";

        public Vector2 MovementDirection { get; private set; }

        private Rigidbody2D _rigidbody;
        private IMovementSpeedSource _speedSource;
        private IAdditionalMovementSource _additionalMovement;
        private System.Func<bool> _mouseMovementEnabled;
        private System.Func<Vector2?> _pointerScreenPosition;
        private Camera _inputCamera;
        private float _mouseDeadzoneWorldUnits;
        private IMovementInputSource _inputSource;

        /// <summary>Null restores the configured keyboard/mouse controls; no gameplay speed or physics rule changes.</summary>
        public void ConfigureInputSource(IMovementInputSource source) => _inputSource = source;

        public void ConfigureMouseMovement(System.Func<bool> mouseMovementEnabled, Camera inputCamera, float deadzoneWorldUnits,
            System.Func<Vector2?> pointerScreenPosition = null)
        {
            _mouseMovementEnabled = mouseMovementEnabled ?? throw new System.ArgumentNullException(nameof(mouseMovementEnabled));
            _inputCamera = inputCamera ?? throw new System.ArgumentNullException(nameof(inputCamera));
            if (deadzoneWorldUnits < 0f) throw new System.ArgumentOutOfRangeException(nameof(deadzoneWorldUnits));
            _mouseDeadzoneWorldUnits = deadzoneWorldUnits;
            _pointerScreenPosition = pointerScreenPosition ?? (() => Mouse.current == null ? (Vector2?)null : Mouse.current.position.ReadValue());
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _speedSource = GetComponent<IMovementSpeedSource>();
            _additionalMovement = GetComponent<IAdditionalMovementSource>();

            if (spawnPoint != null)
                transform.position = spawnPoint.position;

        }

        private void OnEnable()
        {
            moveAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
        }

        private void FixedUpdate()
        {
            var rawInput = ReadMovementInput();
            var isRunning = runController.Model.State == RunState.Running;
            MovementDirection = isRunning ? rawInput.normalized : Vector2.zero;
            var speed = _speedSource != null ? _speedSource.MovementSpeed : 0f;

            var additional = _additionalMovement?.TickAdditionalMovement(Time.fixedDeltaTime, isRunning) ?? Vector2.zero;
            _rigidbody.linearVelocity = MovementVelocityCalculator.Calculate(rawInput, speed, isRunning) + additional;
        }

        private Vector2 ReadMovementInput()
        {
            if (_inputSource != null) return _inputSource.ReadDirection();
            var screenPosition = _pointerScreenPosition?.Invoke();
            if (_mouseMovementEnabled?.Invoke() != true || !screenPosition.HasValue || _inputCamera == null)
                return moveAction.action.ReadValue<Vector2>();

            var playerScreenDepth = _inputCamera.WorldToScreenPoint(transform.position).z;
            var pointerWorldPosition = _inputCamera.ScreenToWorldPoint(new Vector3(screenPosition.Value.x, screenPosition.Value.y, playerScreenDepth));
            return MovementVelocityCalculator.DirectionFromPointer(transform.position, pointerWorldPosition, _mouseDeadzoneWorldUnits);
        }
    }
}
