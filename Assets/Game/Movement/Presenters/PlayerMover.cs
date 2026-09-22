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
            var rawInput = moveAction.action.ReadValue<Vector2>();
            var isRunning = runController.Model.State == RunState.Running;
            MovementDirection = isRunning ? rawInput.normalized : Vector2.zero;
            var speed = _speedSource != null ? _speedSource.MovementSpeed : 0f;

            var additional = _additionalMovement?.TickAdditionalMovement(Time.fixedDeltaTime, isRunning) ?? Vector2.zero;
            _rigidbody.linearVelocity = MovementVelocityCalculator.Calculate(rawInput, speed, isRunning) + additional;
        }
    }
}
