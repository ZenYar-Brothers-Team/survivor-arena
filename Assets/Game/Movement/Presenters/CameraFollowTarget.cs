using UnityEngine;

namespace Game.Movement
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollowTarget : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        public Transform Target => target;

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
            transform.position = new Vector3(targetPosition.x, targetPosition.y, cameraPosition.z);
        }
    }
}
