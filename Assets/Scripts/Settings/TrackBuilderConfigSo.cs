using System;
using Unity.Mathematics;
using UnityEngine;

namespace FearIndigo.Settings
{
    [CreateAssetMenu(fileName = "New Track Builder Config", menuName = "Settings/Track Builder Config", order = 0)]
    public class TrackBuilderConfigSo : ScriptableObject
    {
        public TrackBuilderConfig data;
    }
    
    [Serializable]
    public struct TrackBuilderConfig
    {
        [HideInInspector] public uint seed;
        public float2 trackBounds;
        public float minTrackWidth;
        public float maxTrackWidth;
        public int numRandomPoints;
        public float minRandomPointDistance;
        public float maxXDisplacement;
        public float maxYDisplacement;
        public float minPointDistance;
        public float minCornerAngle;
        public int maxIterations;
        public int maxRandomPointIterations;
        public int maxConvexHullIterations;
        public int maxDisplacementIterations;
    }
}