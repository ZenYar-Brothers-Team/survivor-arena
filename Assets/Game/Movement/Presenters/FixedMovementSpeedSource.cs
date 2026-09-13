using UnityEngine;

namespace Game.Movement
{
    public sealed class FixedMovementSpeedSource : MonoBehaviour, IMovementSpeedSource
    {
        [SerializeField]
        private float speed = 3f;

        public float MovementSpeed => speed;
    }
}
