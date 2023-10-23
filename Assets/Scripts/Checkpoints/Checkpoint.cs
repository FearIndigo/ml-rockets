using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class Checkpoint : CheckpointBase
    {
        public LineRenderer lineRenderer;
        public EdgeCollider2D edgeCollider;
        public float colliderWidth;
        public Color activeColor;
        public Color nextActiveColor;
        public Color inactiveColor;
        public ContactFilter2D contactFilter;

        /// <summary>
        /// <para>
        /// Update the checkpoint start and end positions.
        /// </para>
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public void UpdateLine(float2 startPoint, float2 endPoint)
        {
            var direction = (Vector2)math.normalize(endPoint - startPoint);
            var radius = colliderWidth / 2f;
            var adjustedStart = new Vector2(startPoint.x, startPoint.y) + direction * radius;
            var adjustedEnd = new Vector2(endPoint.x, endPoint.y) - direction * radius;
            lineRenderer.widthMultiplier = colliderWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[]
            {
                adjustedStart,
                adjustedEnd
            });
            
            edgeCollider.edgeRadius = radius;
            edgeCollider.SetPoints(new List<Vector2>()
            {
                adjustedStart,
                adjustedEnd
            });
        }
        
        protected override void OnStateChanged()
        {
            var color = state switch
            {
                State.Inactive => inactiveColor,
                State.NextActive => nextActiveColor,
                State.Active => activeColor,
                _ => Color.magenta
            };
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
}