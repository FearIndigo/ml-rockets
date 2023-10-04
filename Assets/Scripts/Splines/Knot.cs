using System;
using Unity.Burst;
using Unity.Mathematics;

namespace FearIndigo.Splines
{
    [Serializable]
    [BurstCompile]
    public struct Knot
    {
        public float2 position;
        public float width;

        public Knot(float2 position, float width)
        {
            this.position = position;
            this.width = width;
        }
    }
}