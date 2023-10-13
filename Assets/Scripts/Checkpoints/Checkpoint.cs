using System.Collections.Generic;
using FearIndigo.Ship;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class Checkpoint : CheckpointBase
    {
        public LineRenderer lineRenderer;
        public EdgeCollider2D edgeCollider;
        public float width;
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
            lineRenderer.widthMultiplier = width;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new[]
            {
                new Vector3(startPoint.x, startPoint.y, 0),
                new Vector3(endPoint.x, endPoint.y, 0)
            });
            edgeCollider.edgeRadius = width / 2f;
            edgeCollider.SetPoints(new List<Vector2>()
            {
                new Vector2(startPoint.x, startPoint.y),
                new Vector2(endPoint.x, endPoint.y)
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