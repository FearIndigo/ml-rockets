using System.Collections.Generic;
using FearIndigo.Splines;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Track
{
    public class TrackSpline : MonoBehaviour
    {
        [Header("Config")]
        [Range(0,1)]
        public float alpha = 0.5f;
        public int resolution = 64;

        [Header("Track Lines")]
        public LineRenderer leftLine;
        public LineRenderer rightLine;
        
        [Header("Track Mesh")]
        public MeshFilter meshFilter;
        public CustomCollider2D trackCollider;
        
        [HideInInspector] public Spline centreSpline;
        [HideInInspector] public Spline leftSpline;
        [HideInInspector] public Spline rightSpline;
        [HideInInspector] public NativeArray<float> widths;
        
        private bool _init;
        private Mesh _trackMesh;

        public void Init()
        {
            if(_init) return;
            _trackMesh = new Mesh {name = "Track Mesh"};
            centreSpline = new Spline(alpha);
            leftSpline = new Spline(alpha);
            rightSpline = new Spline(alpha);
            _init = true;
        }
        
        private void OnDestroy()
        {
            Dispose();
            if(_trackMesh) Destroy(_trackMesh);
        }

        private void OnValidate()
        {
            Init();
            centreSpline.alpha = alpha;
            leftSpline.alpha = alpha;
            rightSpline.alpha = alpha;
            
            UpdateMeshes();
        }
        
        public void Dispose()
        {
            centreSpline.Dispose();
            leftSpline.Dispose();
            rightSpline.Dispose();
            if(widths.IsCreated) widths.Dispose();
        }
        
        /// <summary>
        /// <para>
        /// Update track spline.
        /// </para>
        /// </summary>
        /// <param name="newPoints"></param>
        /// <param name="newWidths"></param>
        public void UpdateTrack(float2[] newPoints, float[] newWidths)
        {
            Init();
            Dispose();
            widths = new NativeArray<float>(newWidths, Allocator.Persistent);
            centreSpline.SetPoints(newPoints);
            leftSpline.SetPoints(GetOffCentreSplinePoints(true));
            rightSpline.SetPoints(GetOffCentreSplinePoints(false));
            
            UpdateMeshes();
        }
        
        /// <summary>
        /// <para>
        /// Get points for left or right spline given current centre spline and widths.
        /// </para>
        /// </summary>
        /// <param name="left">Get points for left or right spline.</param>
        private float2[] GetOffCentreSplinePoints(bool left)
        {
            var points = new float2[centreSpline.points.Length];
            for (var i = 0; i < centreSpline.points.Length; i++)
            {
                var p = centreSpline.points[i];
                var offset = centreSpline.GetNormal(i/(float)centreSpline.points.Length) * widths[i] / 2f;
                points[i] = p + (left ? offset : -offset);
            }
            return points;
        }

        /// <summary>
        /// <para>
        /// Update meshes for track.
        /// </para>
        /// </summary>
        private void UpdateMeshes()
        {
            UpdateEdgeLine(leftLine, leftSpline);
            UpdateEdgeLine(rightLine, rightSpline);
            UpdateTrackMesh();
        }

        /// <summary>
        /// <para>
        /// Update edge line renderer positions from spline.
        /// </para>
        /// </summary>
        /// <param name="lineRenderer"></param>
        /// <param name="spline"></param>
        private void UpdateEdgeLine(LineRenderer lineRenderer, Spline spline)
        {
            if(spline.points.Length < 4) return;
            
            lineRenderer.positionCount = resolution;
            for (var i = 0; i < resolution; i++)
            {
                var point = spline.GetCurve(i/(float)resolution);
                lineRenderer.SetPosition(i, new Vector3(point.x, point.y, 0));
            }
        }

        /// <summary>
        /// <para>
        /// Generate track mesh from left and right spline curves.
        /// </para>
        /// </summary>
        private void UpdateTrackMesh()
        {
            if(centreSpline.points.Length < 4) return;
            
            var vertices = new Vector3[resolution * 2];
            var triangles = new int[resolution * 6];
            var shapeGroup = new PhysicsShapeGroup2D();
            
            for (var i = 0; i < resolution; i++)
            {
                // Vertices
                var t0 = i / (float)resolution;
                var left0 = leftSpline.GetCurve(t0);
                var right0 = rightSpline.GetCurve(t0);
                vertices[i * 2 + 0] = new Vector3(left0.x, left0.y, 0);
                vertices[i * 2 + 1] = new Vector3(right0.x, right0.y, 0);
                
                // Triangles
                var triIndex = i * 6;
                var vertIndex = i * 2;
                triangles[triIndex + 0] = (vertIndex + 0) % (resolution * 2);
                triangles[triIndex + 1] = (vertIndex + 2) % (resolution * 2);
                triangles[triIndex + 2] = (vertIndex + 1) % (resolution * 2);
                triangles[triIndex + 3] = (vertIndex + 1) % (resolution * 2);
                triangles[triIndex + 4] = (vertIndex + 2) % (resolution * 2);
                triangles[triIndex + 5] = (vertIndex + 3) % (resolution * 2);
                
                // Physics shapes
                var t1 = (i + 1) % resolution / (float)resolution;
                var left1 = leftSpline.GetCurve(t1);
                var right1 = rightSpline.GetCurve(t1);
                shapeGroup.AddPolygon(new List<Vector2>
                {
                    transform.TransformPoint((Vector2)left0),
                    transform.TransformPoint((Vector2)left1),
                    transform.TransformPoint((Vector2)right1),
                    transform.TransformPoint((Vector2)right0)
                });
            }
            
            _trackMesh.Clear();
            _trackMesh.vertices = vertices;
            _trackMesh.triangles = triangles;
            _trackMesh.RecalculateNormals();
            _trackMesh.RecalculateTangents();
            _trackMesh.Optimize();
            meshFilter.sharedMesh = _trackMesh;
            
            trackCollider.ClearCustomShapes();
            trackCollider.SetCustomShapes(shapeGroup);
        }
    }
}