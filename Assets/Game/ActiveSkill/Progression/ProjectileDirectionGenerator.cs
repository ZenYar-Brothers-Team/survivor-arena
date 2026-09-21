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
            float rotationDegrees = 0f,
            System.Random random = null)
        {
            NumericValidation.ValidateCount(count, nameof(count));
            if (!Enum.IsDefined(typeof(ProjectileLayout), layout))
                throw new ArgumentOutOfRangeException(nameof(layout));
            if (layout == ProjectileLayout.Cross && count != 4 && count != 8)
                throw new ArgumentOutOfRangeException(nameof(count), "Cross layout requires four or eight projectiles.");
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
                        angle = baseAngle + 360f * i / count;
                        break;
                    case ProjectileLayout.Cross:
                        angle = rotationDegrees + 360f * i / count;
                        break;
                    case ProjectileLayout.IndependentRandom:
                        if (random == null) throw new ArgumentNullException(nameof(random));
                        angle = (float)(random.NextDouble() * 360d);
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
