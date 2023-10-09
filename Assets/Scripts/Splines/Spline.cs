using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Splines
{
    [Serializable]
    public class Spline
    {
        [Range(0, 1)] public float alpha;
        private NativeArray<float2> _points;

        public int NumPoints => _points.Length;

        public NativeArray<float2> GetPoints() => _points;
        public float2 GetPoint(int i) => _points[i];

        public Spline(float alpha = 0.5f)
        {
            this.alpha = alpha;
            _points = new NativeArray<float2>(0, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (_points.IsCreated) _points.Dispose();
        }
        
        public void SetPoints(float2[] newPoints)
        {
            Dispose();
            _points = new NativeArray<float2>(newPoints, Allocator.Persistent);
        }

        public float2 GetCurve(float t)
        {
            var output = new NativeArray<float2>(1, Allocator.TempJob);
            var getCurveJob = new GetSplineCurveJob()
            {
                T = t,
                Alpha = alpha,
                Points = _points,
                Output = output,
            };
            getCurveJob.Schedule().Complete();
            var point = output[0];
            output.Dispose();
            return point;
        }

        public float2 GetNormal(float t)
        {
            const float tOffset = 0.0001f;
            var p0 = GetCurve((1f + t - tOffset) % 1f);
            var p2 = GetCurve((1f + t + tOffset) % 1f);

            var tangent = math.normalize(p0 - p2);
            return new float2(-tangent.y, tangent.x);
        }

        public int GetSegmentIndex(float t)
        {
            return (int) (t * _points.Length);
        }

        public float GetSegmentT(float t)
        {
            return (t - (1f / _points.Length) * GetSegmentIndex(t)) / (1f / _points.Length);
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct GetSplineCurveJob : IJob
        {
            [ReadOnly] public float T;
            [ReadOnly] public float Alpha;
            [ReadOnly] public NativeArray<float2> Points;

            [WriteOnly] public NativeArray<float2> Output;

            public void Execute()
            {
                GetSegmentIndex(out var segI);
                GetSegmentT(out var segT);
                GetPoint(segI, segT, out var point);
                Output[0] = point;
            }

            [BurstCompile]
            private void GetSegmentIndex(out int index)
            {
                index = (int) (T * Points.Length);
            }

            [BurstCompile]
            private void GetSegmentT(out float segmentT)
            {
                GetSegmentIndex(out var index);
                segmentT = (T - (1f / Points.Length) * index) / (1f / Points.Length);
            }

            // Evaluates a point at the given t-value from 0 to 1
            [BurstCompile]
            private void GetPoint(in int i, in float t, out float2 point)
            {
                var p0 = Points[(Points.Length + i - 1) % Points.Length];
                var p1 = Points[(Points.Length + i + 0) % Points.Length];
                var p2 = Points[(Points.Length + i + 1) % Points.Length];
                var p3 = Points[(Points.Length + i + 2) % Points.Length];

                // calculate knots
                const float k0 = 0;
                GetKnotInterval(p0, p1, out var k1);
                GetKnotInterval(p1, p2, out var k2);
                k2 += k1;
                GetKnotInterval(p2, p3, out var k3);
                k3 += k2;

                // evaluate the point
                var u = math.lerp(k1, k2, t);
                Remap(k0, k1, p0, p1, u, out var a1);
                Remap(k1, k2, p1, p2, u, out var a2);
                Remap(k2, k3, p2, p3, u, out var a3);
                Remap(k0, k2, a1, a2, u, out var b1);
                Remap(k1, k3, a2, a3, u, out var b2);
                Remap(k1, k2, b1, b2, u, out point);
            }

            [BurstCompile]
            private void GetKnotInterval(in float2 a, in float2 b, out float interval)
            {
                interval = math.pow(math.distancesq(a, b), 0.5f * Alpha);
            }

            [BurstCompile]
            private static void Remap(in float a, in float b, in float2 c, in float2 d, float u, out float2 remapped)
            {
                remapped = math.lerp(c, d, (u - a) / (b - a));
            }
        }
    }
}