using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class FinishLine : Checkpoint
    {
        protected override void OnCheckpointAcquired(ShipController shipController)
        {
            base.OnCheckpointAcquired(shipController);

            Time.timeScale = 0.1f;
            GameManager.timerPaused = true;
            
            Debug.Log("Crossed the finish line!");
            
            shipController.EndEpisode();
        }
    }
}