using UnityEngine;

namespace Game.Movement
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollowTarget : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        private Camera _camera;
        private float _arenaHalfSide;

        public Transform Target => target;

        public void ConfigureBounds(float sideLength)
        {
            if (float.IsNaN(sideLength) || float.IsInfinity(sideLength) || sideLength <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(sideLength));
            _arenaHalfSide = sideLength * 0.5f;
            CenterOnTarget();
        }

        public void ClearBounds() => _arenaHalfSide = 0f;

        private void Awake()
        {
            if (target == null)
            {
                Debug.LogError("Camera follow target is not configured.", this);
                enabled = false;
                return;
            }

            CenterOnTarget();
        }

        private void LateUpdate()
        {
            CenterOnTarget();
        }

        public void CenterOnTarget()
        {
            if (target == null)
                return;

            var cameraPosition = transform.position;
            var targetPosition = target.position;
            transform.position = ClampPosition(new Vector3(targetPosition.x, targetPosition.y, cameraPosition.z));
        }

        /// <summary>DECISION-0101: keep the entire viewport over the field, including during render-only shake.</summary>
        public Vector3 ClampPosition(Vector3 desired)
        {
            if (_arenaHalfSide <= 0f) return desired;
            if (_camera == null) _camera = GetComponent<Camera>();

            var halfHeight = _camera.orthographicSize;
            var halfWidth = halfHeight * _camera.aspect;
            var maxX = Mathf.Max(0f, _arenaHalfSide - halfWidth);
            var maxY = Mathf.Max(0f, _arenaHalfSide - halfHeight);
            return new Vector3(Mathf.Clamp(desired.x, -maxX, maxX),
                Mathf.Clamp(desired.y, -maxY, maxY), desired.z);
        }
    }
}
