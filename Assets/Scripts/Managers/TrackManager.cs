using System;
using FearIndigo.Track;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FearIndigo.Managers
{
    public class TrackManager : SubManager
    {
        public bool randomConfig;
        public int trackConfigIndex;
        public TrackBuilderConfigSo[] trackConfigs;
        public TrackSpline trackSpline;
        public uint seed;

        public TrackBuilderConfigSo TrackConfig => trackConfigs?.Length > 0  ? trackConfigs[trackConfigIndex % trackConfigs.Length] : null;
        
        public float[] GetWidths()
        {
            var widths = trackSpline.widths;
            return widths.IsCreated ? widths.ToArray() : Array.Empty<float>();
        }
        public float2[] GetCentreSplinePoints()
        {
            var points = trackSpline.centreSpline.points;
            return points.IsCreated ? points.ToArray() : Array.Empty<float2>();
        }

        /// <summary>
        /// <para>
        /// Generate a new random seed.
        /// </para>
        /// </summary>
        public void RandomizeSeed()
        {
            seed = (uint)Random.Range(0, int.MaxValue);
        }
        
        ///<summary>
        /// <para>
        /// Randomize seed before generating track.
        /// </para>
        /// </summary>
        public void GenerateRandomTrack()
        {
            RandomizeSeed();
            GenerateTrack();
        }

        ///<summary>
        /// <para>
        /// Generate track.
        /// </para>
        /// </summary>
        public void GenerateTrack()
        {
            if (randomConfig)
                trackConfigIndex = Random.Range(0, trackConfigs.Length);
            
            if (Academy.Instance.IsCommunicatorOn)
                trackConfigIndex = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("track_config_index", trackConfigIndex);
            
            TrackConfig.data.seed = seed;
            TrackBuilder.GenerateTrack(ref TrackConfig.data, out var newPoints, out var newWidths);
            trackSpline.UpdateTrack(newPoints, newWidths);
        }

        private void OnDrawGizmos()
        {
            DrawTrackBoundsGizmos();
        }

        ///<summary>
        /// <para>
        /// Draw track bounds gizmos.
        /// </para>
        /// </summary>
        private void DrawTrackBoundsGizmos()
        {
            if(!TrackConfig) return;

            var origin = (Vector2)transform.localPosition;
            var halfBounds = TrackConfig.data.trackBounds / 2f;
            var points = new Vector3[]
            {
                new Vector2(-halfBounds.x, -halfBounds.y) + origin,
                new Vector2(-halfBounds.x, halfBounds.y) + origin,
                new Vector2(halfBounds.x, halfBounds.y) + origin,
                new Vector2(halfBounds.x, -halfBounds.y) + origin
            };

            for (var i = 0; i < points.Length; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
            }
        }
    }
}

