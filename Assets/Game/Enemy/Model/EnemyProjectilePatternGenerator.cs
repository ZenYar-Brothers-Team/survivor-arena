using System;
using UnityEngine;

namespace Game.Enemy
{
    public static class EnemyProjectilePatternGenerator
    {
        public static EnemyShotCommand[] Create(EnemyAttackProfile profile, Vector2 aimDirection, float rotationDegrees = 0f)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            if (aimDirection.sqrMagnitude <= Mathf.Epsilon)
                aimDirection = Vector2.right;

            var count = profile.Pattern == EnemyProjectilePattern.Cross ? 4 : profile.ProjectileCount;
            if (profile.Pattern == EnemyProjectilePattern.Single ||
                profile.Pattern == EnemyProjectilePattern.Burst ||
                profile.Pattern == EnemyProjectilePattern.Explosive)
                count = 1;

            var baseAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg + rotationDegrees;
            var commands = new EnemyShotCommand[count];
            for (var i = 0; i < count; i++)
            {
                float angle;
                switch (profile.Pattern)
                {
                    case EnemyProjectilePattern.Fan:
                        angle = count == 1
                            ? baseAngle
                            : baseAngle - profile.SpreadDegrees * 0.5f + profile.SpreadDegrees * i / (count - 1);
                        break;
                    case EnemyProjectilePattern.Ring:
                    case EnemyProjectilePattern.Cross:
                    case EnemyProjectilePattern.Spiral:
                        angle = baseAngle + 360f * i / count;
                        break;
                    default:
                        angle = baseAngle;
                        break;
                }
                var radians = angle * Mathf.Deg2Rad;
                commands[i] = new EnemyShotCommand(
                    new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)),
                    profile.Pattern == EnemyProjectilePattern.Explosive);
            }
            return commands;
        }
    }
}
