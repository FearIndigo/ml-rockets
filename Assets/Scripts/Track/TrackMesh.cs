using System.Collections.Generic;
using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Track
{
    public class TrackMesh : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public MeshFilter trackObservationMeshFilter;
        public CustomCollider2D trackCollider;
        public EdgeCollider2D leftEdge;
        public EdgeCollider2D rightEdge;
        
        public void SetMesh(Mesh mesh)
        {
            meshFilter.sharedMesh = trackObservationMeshFilter.sharedMesh = mesh;
        }

        public void SetColliderShape(PhysicsShapeGroup2D shapeGroup)
        {
            trackCollider.ClearCustomShapes();
            trackCollider.SetCustomShapes(shapeGroup);
        }

        public void UpdateEdgeColliders(List<Vector2> leftPoints, List<Vector2> rightPoints)
        {
            leftEdge.SetPoints(leftPoints);
            rightEdge.SetPoints(rightPoints);
        }
    }
}
