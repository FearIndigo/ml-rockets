using UnityEngine;

namespace FearIndigo.Utility
{
    public static class InverseCast2D
    {
        public static bool Cast(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, int layerMaskTest)
        {
            var maxIterations = 100;
            var castDistance = distance;
            direction = direction.normalized;
            origin += direction * distance;
            direction *= -1f;
            var wasHit = false;
            var circleCast = radius > 0f;
            var result = results[0];
            while (castDistance > 0f)
            {
                maxIterations--;
                var hitCount = circleCast
                    ? Physics2D.CircleCastNonAlloc(origin, radius, direction, results, castDistance, layerMask)
                    : Physics2D.RaycastNonAlloc(origin, direction, results, castDistance, layerMask);
                if (hitCount > 0)
                {
                    origin += direction * results[0].distance;
                    castDistance -= results[0].distance;
                    if (!Physics2D.OverlapPoint(origin - direction * radius, layerMaskTest))
                    {
                        wasHit = true;
                        result = results[0];
                        result.distance = castDistance;
                        result.fraction = castDistance / distance;
                    }
                }

                if (hitCount == 0 || maxIterations == 0) break;
            }

            results[0] = result;
            
            return wasHit;
        }
    }
}