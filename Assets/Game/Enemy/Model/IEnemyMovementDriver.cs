using UnityEngine;
namespace Game.Enemy
{
    public interface IEnemyMovementDriver
    {
        EnemyMovementFrame Tick(Vector2 position, Vector2 player, float speed, float deltaTime, bool running);
    }
}
