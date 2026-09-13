using Game.Run;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference moveAction;

        [SerializeField]
        private RunController runController;

        [SerializeField]
        private Transform spawnPoint;

        private Rigidbody2D _rigidbody;
        private IMovementSpeedSource _speedSource;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _speedSource = GetComponent<IMovementSpeedSource>();

            if (spawnPoint != null)
                transform.position = spawnPoint.position;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer.sprite == null)
                spriteRenderer.sprite = CreatePlaceholderSprite();
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
            var speed = _speedSource != null ? _speedSource.MovementSpeed : 0f;

            _rigidbody.linearVelocity = MovementVelocityCalculator.Calculate(rawInput, speed, isRunning);
        }

        private static Sprite CreatePlaceholderSprite()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
