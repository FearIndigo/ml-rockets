using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FearIndigo.Splines
{
    public class TrackSpline : MonoBehaviour
    {
        [Range(0, 1)]
        public float debugT;
        [Range(0,1)]
        public float alpha = 0.5f;
        public int resolution = 64;

        public NativeArray<float> widths;
        [HideInInspector] public Spline centreSpline;
        [HideInInspector] public Spline leftSpline;
        [HideInInspector] public Spline rightSpline;

        private bool _init;
        
        public void Init()
        {
            if(_init) return;
            centreSpline = new Spline(alpha);
            leftSpline = new Spline(alpha);
            rightSpline = new Spline(alpha);
            _init = true;
        }
        
        private void OnDestroy()
        {
            Dispose();
        }

        private void OnValidate()
        {
            Init();
            centreSpline.alpha = alpha;
            leftSpline.alpha = alpha;
            rightSpline.alpha = alpha;
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
        /// <param name="newPoints"></param>
        /// <param name="newWidths"></param>
        /// </summary>
        public void UpdateTrack(float2[] newPoints, float[] newWidths)
        {
            Init();
            Dispose();
            centreSpline.SetPoints(newPoints);
            widths = new NativeArray<float>(newWidths, Allocator.Persistent);
        }

        private void OnDrawGizmos()
        {
            DrawDebugT(centreSpline);
            DrawPointsGizmos(centreSpline.points);
            DrawSplineGizmos(centreSpline);
        }
        
        private void DrawDebugT(Spline spline)
        {
            if(spline.points.Length < 4) return;

            var curvePos = spline.GetCurve(debugT);
            var pos = new Vector3(curvePos.x, curvePos.y, 0);
            Gizmos.DrawSphere(pos, 2f);
#if UNITY_EDITOR
            var i = spline.GetSegmentIndex(debugT);
            var t = spline.GetSegmentT(debugT);
            Handles.Label(pos, $"index: {i}, t: {t}");
#endif
        }
        
        private void DrawPointsGizmos(NativeArray<float2> points)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var p0 = points[i];
                var pos = new Vector3(p0.x, p0.y, 0);
                Gizmos.DrawSphere(pos, 0.5f);
#if UNITY_EDITOR
                Handles.Label(pos, $"Point: {i}");
#endif
            }
        }
        
        private void DrawSplineGizmos(Spline spline)
        {
            if(spline.points.Length < 4) return;

            var prev = spline.points[0];
            for (var i = 0; i < resolution; i++)
            {
                var t = (i + 1f) / resolution;
                var current = spline.GetCurve(t);
                Gizmos.DrawLine(new Vector3(prev.x, prev.y, 0), new Vector3(current.x, current.y, 0));
                prev = current;
            }
        }
    }
}