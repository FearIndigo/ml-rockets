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
            if(randomSeedOnReset) trackManager.RandomizeSeed();
            trackManager.GenerateTrack();
            checkpointManager.CreateCheckpoints();
            shipManager.SpawnShips();
            timerManager.Reset();
            cameraManager.SetCameraTarget(shipManager.MainShip?.transform);
        }

        /// <summary>
        /// Reset using the current seed.
        /// </summary>
        public void ResetCurrent()
        {
            checkpointManager.CreateCheckpoints();
            shipManager.SpawnShips();
            timerManager.Reset();
            cameraManager.SetCameraTarget(shipManager.MainShip?.transform);
        }
    }
}

