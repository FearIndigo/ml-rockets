using Cinemachine;
using FearIndigo.Checkpoints;
using FearIndigo.Settings;
using FearIndigo.Ship;
using FearIndigo.Splines;
using FearIndigo.Track;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FearIndigo.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Camera")]
        public CinemachineVirtualCamera virtualCam;
        
        [Header("Track")]
        public TrackBuilderConfigSo trackConfig;
        public TrackSpline trackSpline;

        [Header("Prefabs")]
        public ShipController shipPrefab;
        public Checkpoint checkpointPrefab;

        [Header("Gameplay")]
        public ShipController ship;
        public Checkpoint[] checkpoints;
        public int activeCheckpointId;
        
        public void Start()
        {
            GenerateRandomTrack();

            if (ship)
            {
                Destroy(ship.gameObject);
            }
            ship = Instantiate(shipPrefab, transform);
            ship.Teleport(trackSpline.GetPoint(0));

            if (virtualCam) virtualCam.Follow = ship.transform;
        }

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
            CreateCheckpoints(newPoints);
        }

        ///<summary>
        /// <para>
        /// Create checkpoints from positions.
        /// </para>
        /// <param name="positions"></param>
        /// </summary>
        private void CreateCheckpoints(float2[] positions)
        {
            if(!Application.isPlaying) return;
            
            if (checkpoints != null)
            {
                foreach (var checkpoint in checkpoints)
                {
                    Destroy(checkpoint.gameObject);
                }
            }

            checkpoints = new Checkpoint[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                checkpoints[i] = Instantiate(checkpointPrefab, transform);
                checkpoints[i].Init(this, i, positions[i]);
            }

            SetActiveCheckpoint(0);
        }

        /// <summary>
        /// <para>
        /// Sets the new active checkpoint.
        /// </para>
        /// </summary>
        /// <param name="checkpointId"></param>
        public void SetActiveCheckpoint(int checkpointId)
        {
            checkpointId %= checkpoints.Length;
            checkpoints[activeCheckpointId].SetActive(Checkpoint.State.Inactive);
            checkpoints[checkpointId].SetActive(Checkpoint.State.Active);
            checkpoints[(checkpointId + 1) % checkpoints.Length].SetActive(Checkpoint.State.NextActive);
            activeCheckpointId = checkpointId;
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
            var origin = transform.position;
            origin.z = 0;
            var halfBounds = trackConfig.data.trackBounds / 2f;
            var points = new Vector3[]
            {
                new Vector3(-halfBounds.x, -halfBounds.y, 0) + origin,
                new Vector3(-halfBounds.x, halfBounds.y, 0) + origin,
                new Vector3(halfBounds.x, halfBounds.y, 0) + origin,
                new Vector3(halfBounds.x, -halfBounds.y, 0) + origin
            };

            for (var i = 0; i < points.Length; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
            }
        }
    }
}

