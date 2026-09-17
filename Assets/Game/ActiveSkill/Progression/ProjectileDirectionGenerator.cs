using System;
using Game.Content;
using UnityEngine;

namespace Game.ActiveSkill
{
    public static class ProjectileDirectionGenerator
    {
        public static Vector2[] Create(
            ProjectileLayout layout,
            int count,
            Vector2 aimDirection,
            float spreadDegrees = 0f,
            float rotationDegrees = 0f)
        {
            NumericValidation.ValidateCount(count, nameof(count));
            if (!Enum.IsDefined(typeof(ProjectileLayout), layout))
                throw new ArgumentOutOfRangeException(nameof(layout));
            if (aimDirection.sqrMagnitude <= Mathf.Epsilon)
                aimDirection = Vector2.right;

            var baseAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg + rotationDegrees;
            var directions = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                float angle;
                switch (layout)
                {
                    case ProjectileLayout.Single:
                        angle = baseAngle;
                        break;
                    case ProjectileLayout.Fan:
                        angle = count == 1
                            ? baseAngle
                            : baseAngle - spreadDegrees * 0.5f + spreadDegrees * i / (count - 1);
                        break;
                    case ProjectileLayout.Ring:
                    case ProjectileLayout.Cross:
                        angle = baseAngle + 360f * i / count;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(layout));
                }

                var radians = angle * Mathf.Deg2Rad;
                directions[i] = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            }
            return directions;
        }
    }
}
