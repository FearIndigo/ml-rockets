using System;
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
            timerManager.Reset();
            trackManager.GenerateRandomTrack();
            checkpointManager.CreateCheckpoints();
            shipManager.SpawnShips();
            cameraManager.UpdateCameraTarget();
        }
    }
}

