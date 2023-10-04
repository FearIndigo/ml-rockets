using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Splines
{
    [BurstCompile]
    public class Spline : MonoBehaviour
    {
        [Range(0,1)]
        public float alpha = 0.5f;
        public int resolution = 64;
        
        private NativeArray<Knot> _knots;

        private void OnDestroy()
        {
            _knots.Dispose();
        }

        [BurstCompile]
        public void SetKnots(NativeArray<Knot> knots)
        {
            _knots.Dispose();
            _knots = new NativeArray<Knot>(knots, Allocator.Persistent);
        }

        [BurstCompile]
        private float2 GetCurve(float t)
        {
            var i = GetSegmentIndex(t);
            var segT = GetSegmentT(t);
            return GetPoint(i, segT);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSegmentIndex(float t)
        {
            return (int)(t * _knots.Length);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSegmentT(float t)
        {
            return t % (1f / _knots.Length) * _knots.Length;
        }
        
        // Evaluates a point at the given t-value from 0 to 1
        [BurstCompile]
        private float2 GetPoint(int i, float t)
        {
            var p0 = _knots[(_knots.Length + i - 1) % _knots.Length].position;
            var p1 = _knots[(_knots.Length + i + 0) % _knots.Length].position;
            var p2 = _knots[(_knots.Length + i + 1) % _knots.Length].position;
            var p3 = _knots[(_knots.Length + i + 2) % _knots.Length].position;
            
            // calculate knots
            const float k0 = 0;
            var k1 = GetKnotInterval(p0, p1);
            var k2 = GetKnotInterval(p1, p2) + k1;
            var k3 = GetKnotInterval(p2, p3) + k2;

            // evaluate the point
            var u = math.lerp(k1, k2, t);
            var a1 = Remap(k0, k1, p0, p1, u);
            var a2 = Remap(k1, k2, p1, p2, u);
            var a3 = Remap(k2, k3, p2, p3, u);
            var b1 = Remap(k0, k2, a1, a2, u);
            var b2 = Remap(k1, k3, a2, a3, u);
            return Remap(k1, k2, b1, b2, u);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetKnotInterval(float2 a, float2 b)
        {
            return math.pow(math.distancesq(a, b), 0.5f * alpha);
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float2 Remap(float a, float b, float2 c, float2 d, float u)
        {
            return math.lerp(c, d, (u - a) / (b - a));
        }

        private void OnDrawGizmos()
        {
            if(_knots.Length < 4) return;

            var prev = _knots[0].position;
            for (var i = 0; i < resolution; i++)
            {
                var t = (i + 1f) / resolution;
                var current = GetCurve(t);
                Gizmos.DrawLine(new Vector3(prev.x, prev.y, 0), new Vector3(current.x, current.y, 0));
                prev = current;
            }
        }
    }
}