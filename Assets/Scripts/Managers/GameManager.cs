using System;
using Unity.MLAgents;
using UnityEngine;

namespace FearIndigo.Managers
{
    [RequireComponent(typeof(TimerManager))]
    [RequireComponent(typeof(ShipManager))]
    [RequireComponent(typeof(CameraManager))]
    [RequireComponent(typeof(TrackManager))]
    [RequireComponent(typeof(CheckpointManager))]
    public class GameManager : MonoBehaviour
    {
        public bool randomSeedOnReset;
        [HideInInspector] public TimerManager timerManager;
        [HideInInspector] public ShipManager shipManager;
        [HideInInspector] public CameraManager cameraManager;
        [HideInInspector] public TrackManager trackManager;
        [HideInInspector] public CheckpointManager checkpointManager;

        private void Awake()
        {
            timerManager = GetComponent<TimerManager>();
            shipManager = GetComponent<ShipManager>();
            cameraManager = GetComponent<CameraManager>();
            trackManager = GetComponent<TrackManager>();
            checkpointManager = GetComponent<CheckpointManager>();
        }

        public void Start()
        {
            Reset();
        }

        /// <summary>
        /// <para>
        /// Reset the game.
        /// </para>
        /// </summary>
        public void Reset()
        {
            if (Academy.Instance.IsCommunicatorOn)
                trackManager.trackConfigIndex = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("track_config_index", trackManager.trackConfigIndex);
            
            if(randomSeedOnReset) trackManager.RandomizeSeed();
            trackManager.GenerateTrack();
            checkpointManager.CreateCheckpoints();
            shipManager.SpawnShips();
            timerManager.Reset();
            cameraManager.SetCameraTarget(shipManager.MainShip?.transform);
        }
    }
}

