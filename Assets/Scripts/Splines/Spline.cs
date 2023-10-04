using System;
using Unity.Burst;
using Unity.Collections;

namespace FearIndigo.Splines
{
    [Serializable]
    [BurstCompile]
    public class Spline
    {
        private NativeArray<Knot> _knots;

        [BurstCompile]
        public void SetKnots(NativeArray<Knot> knots)
        {
            _knots = knots;
        }

        [BurstCompile]
        public void UpdateSpline()
        {
            
        }
    }
}