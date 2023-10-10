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
        
        private bool _init;
        private NativeArray<float> _widths;
        private Spline _centreSpline;
        private Spline _leftSpline;
        private Spline _rightSpline;
        private Mesh _trackMesh;
        
        public float2 GetCentreSplinePoint(int i) => _centreSpline.GetPoint(i);
        public float2 GetLeftSplinePoint(int i) => _leftSpline.GetPoint(i);
        public float2 GetRightSplinePoint(int i) => _rightSpline.GetPoint(i);
        
        public void Init()
        {
            if(_init) return;
            _trackMesh = new Mesh {name = "Track Mesh"};
            _centreSpline = new Spline(alpha);
            _leftSpline = new Spline(alpha);
            _rightSpline = new Spline(alpha);
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
            _centreSpline.alpha = alpha;
            _leftSpline.alpha = alpha;
            _rightSpline.alpha = alpha;
            
            UpdateMeshes();
        }
        
        public void Dispose()
        {
            _centreSpline.Dispose();
            _leftSpline.Dispose();
            _rightSpline.Dispose();
            if(_widths.IsCreated) _widths.Dispose();
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
            _widths = new NativeArray<float>(newWidths, Allocator.Persistent);
            _centreSpline.SetPoints(newPoints);
            _leftSpline.SetPoints(GetOffCentreSplinePoints(true));
            _rightSpline.SetPoints(GetOffCentreSplinePoints(false));
            
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
            var points = new float2[_centreSpline.NumPoints];
            for (var i = 0; i < _centreSpline.NumPoints; i++)
            {
                var p = _centreSpline.GetPoint(i);
                var offset = _centreSpline.GetNormal(i/(float)_centreSpline.NumPoints) * _widths[i] / 2f;
                points[i] = p + (left ? -offset : offset);
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
            UpdateEdgeLine(leftLine, _leftSpline);
            UpdateEdgeLine(rightLine, _rightSpline);
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
            if(spline.NumPoints < 4) return;
            
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
            if(_centreSpline.NumPoints < 4) return;
            
            var vertices = new Vector3[resolution * 2];
            var triangles = new int[resolution * 6];
            var shapeGroup = new PhysicsShapeGroup2D();
            
            for (var i = 0; i < resolution; i++)
            {
                // Vertices
                var t0 = i / (float)resolution;
                var left0 = _leftSpline.GetCurve(t0);
                var right0 = _rightSpline.GetCurve(t0);
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
                var left1 = _leftSpline.GetCurve(t1);
                var right1 = _rightSpline.GetCurve(t1);
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