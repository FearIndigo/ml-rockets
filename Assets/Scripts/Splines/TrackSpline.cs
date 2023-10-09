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
        [Range(0,1)]
        public float alpha = 0.5f;
        public int resolution = 64;

        public LineRenderer leftLine;
        public LineRenderer rightLine;
        
        private NativeArray<float> _widths;
        private Spline _centreSpline;
        private Spline _leftSpline;
        private Spline _rightSpline;

        private bool _init;
        
        public float2 GetPoint(int i) => _centreSpline.GetPoint(i);
        
        public void Init()
        {
            if(_init) return;
            _centreSpline = new Spline(alpha);
            _leftSpline = new Spline(alpha);
            _rightSpline = new Spline(alpha);
            _init = true;
        }
        
        private void OnDestroy()
        {
            Dispose();
        }

        private void OnValidate()
        {
            Init();
            _centreSpline.alpha = alpha;
            _leftSpline.alpha = alpha;
            _rightSpline.alpha = alpha;
            
            UpdateLine(leftLine, _leftSpline);
            UpdateLine(rightLine, _rightSpline);
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
        /// <param name="newPoints"></param>
        /// <param name="newWidths"></param>
        /// </summary>
        public void UpdateTrack(float2[] newPoints, float[] newWidths)
        {
            Init();
            Dispose();
            _widths = new NativeArray<float>(newWidths, Allocator.Persistent);
            _centreSpline.SetPoints(newPoints);
            _leftSpline.SetPoints(GetOffCentreSplinePoints(true));
            _rightSpline.SetPoints(GetOffCentreSplinePoints(false));
            
            UpdateLine(leftLine, _leftSpline);
            UpdateLine(rightLine, _rightSpline);
        }

        /// <summary>
        /// <para>
        /// Update line renderer positions from spline.
        /// </para>
        /// </summary>
        /// <param name="lineRenderer"></param>
        /// <param name="spline"></param>
        private void UpdateLine(LineRenderer lineRenderer, Spline spline)
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
        /// Get points for left or right spline given current centre spline and widths.
        /// </para>
        /// <param name="left">Get points for left or right spline.</param>
        /// </summary>
        public float2[] GetOffCentreSplinePoints(bool left)
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
    }
}