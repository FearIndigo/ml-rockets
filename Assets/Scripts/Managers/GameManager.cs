using System.Collections.Generic;
using Cinemachine;
using FearIndigo.Checkpoints;
using FearIndigo.Settings;
using FearIndigo.Ship;
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
        public FinishLine finishLinePrefab;

        [Header("Gameplay")]
        public bool timerPaused;
        public float timer;
        public int checkpointsAcquired;
        public Dictionary<int, float> checkpointSplits = new Dictionary<int, float>();
        public ShipController ship;
        public CheckpointBase[] checkpoints;
        public int activeCheckpointId;
        
        public Vector2 ActiveCheckpointDirection(Vector2 position) =>
            (Vector2)checkpoints[activeCheckpointId].transform.position - position;
        public Vector2 NextActiveCheckpointDirection(Vector2 position) => activeCheckpointId != checkpoints.Length - 1
            ? (Vector2)checkpoints[activeCheckpointId + 1].transform.position - position
            : Vector2.zero;
        
        public void Start()
        {
            SpawnShip(float2.zero);
        }

        /// <summary>
        /// <para>
        /// Reset the game.
        /// </para>
        /// </summary>
        public void Reset()
        {
            timerPaused = false;
            timer = 0f;
            checkpointsAcquired = 0;
            checkpointSplits.Clear();
            Time.timeScale = 1f;

            GenerateRandomTrack();
            SpawnShip(trackSpline.GetCentreSplinePoint(0));
        }

        /// <summary>
        /// <para>
        /// Spawn new ship at position, or teleport current ship to position.
        /// </para>
        /// </summary>
        /// <param name="position"></param>
        public void SpawnShip(float2 position)
        {
            if (!ship)
            {
                ship = Instantiate(shipPrefab, transform);
                if (virtualCam) virtualCam.Follow = ship.transform;
            }
            ship.Teleport(position);
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
        /// </summary>
        /// <param name="positions"></param>
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
            
            checkpoints = new CheckpointBase[positions.Length];
            for (var i = 0; i < positions.Length - 1; i++)
            {
                var checkpoint = Instantiate(checkpointPrefab, transform);
                checkpoint.Init(i, positions[i + 1]);
                checkpoints[i] = checkpoint;
            }
            var finishLine = Instantiate(finishLinePrefab, transform);
            finishLine.Init(checkpoints.Length - 1, positions[0]);
            finishLine.UpdateLine(trackSpline.GetLeftSplinePoint(0) - positions[0], trackSpline.GetRightSplinePoint(0) - positions[0]);
            checkpoints[^1] = finishLine;
            activeCheckpointId = 0;
            SetActiveCheckpoint(0);
        }

        /// <summary>
        /// <para>
        /// Sets the active checkpoint, the next active checkpoint and deactivates the previously active checkpoint.
        /// </para>
        /// </summary>
        /// <param name="checkpointId"></param>
        public void SetActiveCheckpoint(int checkpointId)
        {
            checkpoints[activeCheckpointId].SetState(CheckpointBase.State.Inactive);
            
            if (checkpointId >= checkpoints.Length) return;
            checkpoints[checkpointId].SetState(CheckpointBase.State.Active);
            activeCheckpointId = checkpointId;
            
            if (activeCheckpointId >= checkpoints.Length - 1) return;
            checkpoints[activeCheckpointId + 1].SetState(CheckpointBase.State.NextActive);
        }

        /// <summary>
        /// <para>
        /// Set the timer split for the checkpointId.
        /// </para>
        /// </summary>
        /// <param name="checkpointId"></param>
        public void UpdateCheckpointSplit(int checkpointId)
        {
            checkpointSplits.TryAdd(checkpointId, timer);
        }

        public void FixedUpdate()
        {
            UpdateTimer();
        }

        ///<summary>
        /// <para>
        /// Increase time on timer if not paused.
        /// </para>
        /// </summary>
        private void UpdateTimer()
        {
            if(timerPaused) return;
            timer += Time.deltaTime;
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

