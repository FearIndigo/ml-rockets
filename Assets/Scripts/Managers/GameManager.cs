using FearIndigo.Settings;
using FearIndigo.Splines;
using FearIndigo.Track;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FearIndigo.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Track")]
        public TrackBuilderConfigSo trackConfig;
        public TrackSpline trackSpline;

        /// <summary>
        /// <para>
        /// Generate a new random seed.
        /// </para>
        /// </summary>
        public void RandomizeSeed()
        {
            trackConfig.data.seed = (uint)Random.Range(0, int.MaxValue);
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
            TrackBuilder.GenerateTrack(ref trackConfig.data, out var newPoints, out var newWidths);
            trackSpline.UpdateTrack(newPoints, newWidths);
        }

        public void OnDrawGizmos()
        {
            DrawTrackBoundsGizmos();
        }

        ///<summary>
        /// <para>
        /// Draw gizmos for track bounds.
        /// </para>
        /// </summary>
        private void DrawTrackBoundsGizmos()
        {
            var halfBounds = trackConfig.data.trackBounds / 2f;
            var points = new Vector3[]
            {
                new Vector3(-halfBounds.x, -halfBounds.y, 0),
                new Vector3(-halfBounds.x, halfBounds.y, 0),
                new Vector3(halfBounds.x, halfBounds.y, 0),
                new Vector3(halfBounds.x, -halfBounds.y, 0)
            };

            for (var i = 0; i < points.Length; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
            }
        }
    }
}

